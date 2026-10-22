using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Application.Query.CustProfileMS;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.CustProfile;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.SSRToPNR;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class AddFCNumberToPNRCommandHandler(IAzureBlobService azureBlobService, ITableStorageService<MemberDetailsToAddSSR> tableStorageService, CustomerSearchDocumentServiceQueryHandler customerSearchDocumentServiceQueryHandler, ICustomerProfileQuery customerProfileQueryHandler, AddFCNumberToPNRResponseQueryHandler addFCNumberToPNRResponseQueryHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventHandler, AzureStorageCommandHandler azureStorageCommandHandler) : IAddFCNumberToPNRCommand
    {
        private readonly string _containerNameForAddSSRFile = ConfigReader.Get(Constants.Environment.ContainerNameForAddSSRFile);
        private readonly string _archiveContainerNameForAddSSRFile = ConfigReader.Get(Constants.Environment.ArchiveContainerNameForAddSSRFile);
        private readonly string _sasKey = ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtSasKey);
        private readonly string _StorageUrl = ConfigReader.Get(Constants.Environment.BlobConnStringForEnrolledMemberToPromotion);
        private readonly string _stampfcnumbertopnrcontainerUrl = ConfigReader.Get(Constants.Environment.StampfcnumbertopnrcontainerUrl);

        private readonly IAzureBlobService _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        private readonly ITableStorageService<MemberDetailsToAddSSR> _tableStorageService = tableStorageService ?? throw new ArgumentNullException(nameof(tableStorageService));

        private readonly CustomerSearchDocumentServiceQueryHandler _customerSearchDocumentServiceQueryHandler = customerSearchDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(customerSearchDocumentServiceQueryHandler));
        private readonly AddFCNumberToPNRResponseQueryHandler _addFCNumberToPNRResponseQueryHandler = addFCNumberToPNRResponseQueryHandler ?? throw new ArgumentNullException(nameof(addFCNumberToPNRResponseQueryHandler));
        private readonly ICustomerProfileQuery _customerProfileQueryHandler = customerProfileQueryHandler ?? throw new ArgumentNullException(nameof(customerProfileQueryHandler));

        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventHandler));
        private readonly AzureStorageCommandHandler _azureStorageCommandHandler = azureStorageCommandHandler ?? throw new ArgumentNullException(nameof(azureStorageCommandHandler));

        public async Task<string> Handle(AddFCNumberToPNRCommand request, CancellationToken cancellationToken)
        {
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.BlobConnStringForEnrolledMemberToPromotion);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.FCStampingTableName);

            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                if (request.FileName.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.FileName));

                //Archive The Input file
                await _azureStorageCommandHandler.Handle(new AzureStorageCommand()
                {
                    StorageConnection = _StorageUrl,
                    ContainerName = _containerNameForAddSSRFile,
                    ArchiveContainer = _archiveContainerNameForAddSSRFile,
                    BlobName = request.FileName,
                    Action = AzureStorageCommand.Actions.Copy
                }, cancellationToken);

                //Delete The input file
                try
                {
                    Uri blobUri = _stampfcnumbertopnrcontainerUrl.Parse<Uri>();
                    string _storagecontanierUrl = $"{blobUri.Scheme}://{blobUri.Host}";
                    await _azureBlobService.DeleteBlobAsync(_storagecontanierUrl, _containerNameForAddSSRFile, _sasKey, request.FileName, cancellationToken);
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 404)
                {
                    return ex.ErrorCode;
                }

                request.FileName = request.FileName.Replace('/', '_');
                var fileDetails = await _tableStorageService.RetrieveItemsInPartitionAsync(request.FileName, default);
                var currentFileDetails = fileDetails.Where(m => m.Status == MemberDetailsToAddSSR.Statuses.Ingested.ToString());

                if (!currentFileDetails.Any())
                {
                    foreach (var member in request.MemberDeatilsToAddSSR)
                    {
                        await _tableStorageService.InsertOrMergeAsync(
                            FCSTampingDetailMapper.FCSTampingDataMapper(
                                member,
                                request.FileName,
                                MemberDetailsToAddSSR.Statuses.Ingested,
                                new AddFCNumberToPNRQueryResponse()
                            ));
                    }

                    await ProcessMembersAsync(request.MemberDeatilsToAddSSR, request.FileName, cancellationToken);
                }
                else
                {
                    await ProcessMembersAsync(currentFileDetails, request.FileName, cancellationToken);
                }

                //Post Event to trigger Response file generation
                var eventData = new EventDataForResponse() { FileStatus = MemberDetailsToAddSSR.Statuses.Processed.ToString(), FileName = request.FileName };
                var cloudEvent = SSRToPNRCloudEventMapper.CommandToResponseFileEventMapper(eventData, Guid.NewGuid().ToString());
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = cloudEvent.Source }, cancellationToken);
                }
                catch
                {
                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    throw;
                }

                return Constants.Application.Success;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task AddFCNumberToPNRAsync(MemberDetailsToAddSSR memberDeatils, string fileName, CancellationToken cancellationToken)
        {
            AddFCNumberToPNRQueryResponse addSSRResponse = new AddFCNumberToPNRQueryResponse();
            if (new AddFCNumberToPNRDataValidator().Validate(memberDeatils).IsValid)
            {
                //Post Event to Add SSR
                var eventData = SSRtoPNREventDataMapper.MemberDetailsToFQTVSSREventDataMapper(memberDeatils);
                var cloudEvent = SSRToPNRCloudEventMapper.CommandToEventMapper(eventData, Guid.NewGuid().ToString());

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                try
                {
                    await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = cloudEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.OrderMgtServiceBusConnectionString), ServiceBusTopicName = ConfigReader.Get(Constants.Environment.AddSSRTopicEndPoint) }, cancellationToken);
                }
                catch
                {
                    await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                    throw;
                }

                addSSRResponse = await _addFCNumberToPNRResponseQueryHandler.Handle(new AddFCNumberToPNRResponseQuery { CorrelationId = cloudEvent.Id }, cancellationToken);

                memberDeatils.Response = addSSRResponse.Response;
            }
            else
            {
                memberDeatils.Response = Constants.Application.MandatoryInputMissing;
            }

            //insert file data with processed in table
            await _tableStorageService.InsertOrMergeAsync(FCSTampingDetailMapper.FCSTampingDataMapper(memberDeatils, fileName, MemberDetailsToAddSSR.Statuses.Processed, addSSRResponse));
        }
        private async Task ProcessMembersAsync(IEnumerable<MemberDetailsToAddSSR> members,string fileName,CancellationToken cancellationToken)
        {
            foreach (var member in members)
            {
                await ProcessSingleMemberAsync(member, fileName, cancellationToken);
            }
        }
        private async Task ProcessSingleMemberAsync(MemberDetailsToAddSSR member,string fileName,CancellationToken cancellationToken)
        {
            try
            {
                if (member.MemberNumber.IsNullOrWhiteSpace())
                {
                    await InsertFailureAsync(member, fileName, "MemberNumber is Null");
                    return;
                }                

                var searchResult = await _customerSearchDocumentServiceQueryHandler.Handle(
                    new CustomerSearchDocumentServiceQuery
                    {
                        MemberNumber = member.MemberNumber
                    },
                    cancellationToken);

                if (!searchResult.IsSuccess || string.IsNullOrWhiteSpace(searchResult.Value?.Value?.CustomerId))
                {
                    await InsertFailureAsync(member, fileName, searchResult.Error);
                    return;
                }

                var profileResult = await _customerProfileQueryHandler.Handle(
                    new CustomerProfileRequest
                    {
                        CustomerId = searchResult.Value.Value.CustomerId,
                        MemberNumber = member.MemberNumber
                    },
                    cancellationToken);

                if (!profileResult.IsSuccess)
                {
                    await InsertFailureAsync(member, fileName, profileResult.Error);
                    return;
                }

                var preferences = profileResult.Value?.Value?.CustomerProfile?.Preferences;

                if (ShouldStamp(preferences))
                {
                    await AddFCNumberToPNRAsync(member, fileName, cancellationToken);
                }
                else
                {
                    await InsertProcessedAsync(member, fileName, "Skipped");
                }
            }
            catch (Exception ex)
            {
                await InsertFailureAsync(member, fileName, $"Unhandled exception: {ex.Message}");
            }
        }
        private bool ShouldStamp(IEnumerable<Preference> preferences)
        {
            if (preferences == null || !preferences.Any())
                return true;

            var preferredPartner = preferences
                .FirstOrDefault(p => p.PreferenceType == "PreferredPartnerFFP");

            var value = preferredPartner?.PreferenceValue;

            return preferredPartner == null ||
                   string.IsNullOrEmpty(value) ||
                   string.Equals(value, "VS", StringComparison.OrdinalIgnoreCase);
        }
        private async Task InsertFailureAsync(MemberDetailsToAddSSR member,string fileName,string error)
        {
            await _tableStorageService.InsertOrMergeAsync(
                FCSTampingDetailMapper.FCSTampingDataMapper(
                    member,
                    fileName,
                    MemberDetailsToAddSSR.Statuses.Failed,
                   new AddFCNumberToPNRQueryResponse { Response = Constants.Application.Faliure, ErrorMessage = error }));
        }
        private async Task InsertProcessedAsync(MemberDetailsToAddSSR member,string fileName,string message)
        {
            await _tableStorageService.InsertOrMergeAsync(
                FCSTampingDetailMapper.FCSTampingDataMapper(
                    member,
                    fileName,
                    MemberDetailsToAddSSR.Statuses.Processed,
                    new AddFCNumberToPNRQueryResponse { Response = message }));
        }
    }
}
