using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.AzureBlob;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BulkMemberUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Vred;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.VredMs;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.BulkMemberUpdate;


namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.BulkMemberUpdate
{
    public class BulkMemberUpdateCommandHandler(ILogger<BulkMemberUpdateCommandHandler> logger, IAzureBlobService azureBlobService, CustomerSearchDocumentServiceQueryHandler customerSearchDocumentServiceQueryHandler, IVredCustomerDetailsResponseQuery vredCustomerDetailsResponseQuery, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventHandler, AzureStorageCommandHandler azureStorageCommandHandler) : IBulkMemberUpdateCommand
    {
        private readonly string _containerNameForBulkUpload = ConfigReader.Get(Constants.Environment.ContainerNameForBulkUploadInput);
        private readonly string _containerNameForBulkUploadResponse = ConfigReader.Get(Constants.Environment.ContainerNameForBulkUploadOutput);
        private readonly string _archiveContainerNameForBulkUpload = ConfigReader.Get(Constants.Environment.ArchiveContainerNameForBulkUpload);
        private readonly string _sasKey = ConfigReader.Get(Constants.Environment.LoyaltyMbrMgmtSasKey);
        private readonly string _StorageUrl = ConfigReader.Get(Constants.Environment.BlobConnStringForBulkUpload);

        private readonly ILogger<BulkMemberUpdateCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IAzureBlobService _azureBlobService = azureBlobService ?? throw new ArgumentNullException(nameof(azureBlobService));
        private readonly CustomerSearchDocumentServiceQueryHandler _customerSearchDocumentServiceQueryHandler = customerSearchDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(customerSearchDocumentServiceQueryHandler));
        private readonly IVredCustomerDetailsResponseQuery _vredCustomerDetailsResponseQuery = vredCustomerDetailsResponseQuery ?? throw new ArgumentNullException(nameof(vredCustomerDetailsResponseQuery));

        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventHandler));
        private readonly AzureStorageCommandHandler _azureStorageCommandHandler = azureStorageCommandHandler ?? throw new ArgumentNullException(nameof(azureStorageCommandHandler));

        public async Task<Result<Response<BulkMemberUpdateResponses>>> Handle(BulkMemberUpdateRequest request, CancellationToken cancellationToken)
        {
            var sb = new StringBuilder();
            Customer customerDoc = new Customer();
            FlyingClubMember flyingClubMemberDoc = new FlyingClubMember();
            string contents;

            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.MemberNotes.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.MemberNotes));
            if (request.BlobFilePath.IsNullOrWhiteSpace()) throw new ArgumentNullException(nameof(request.BlobFilePath));

            try
            {
                Uri blobUri = request.BlobFilePath.Parse<Uri>();
                string storageAccountUrl = $"{blobUri.Scheme}://{blobUri.Host}";
                string blobName = Path.GetFileName(blobUri.AbsolutePath);
                try
                {
                    await _azureBlobService.DownloadBlobAsync(storageAccountUrl, _containerNameForBulkUpload, blobName, _sasKey, cancellationToken);

                    contents = await _azureBlobService.BlobStringAsync(blobName, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error occurred while trying to download the blob content. {ex.Message}");
                    return Result.Failure<Response<BulkMemberUpdateResponses>>("Blob Not Found");
                }
                var normalizedContents = contents.TrimEnd().Replace("\r\n", "\n");

                var memberNumberArray = normalizedContents.Split('\n').Skip(1);

                var fileDescription = TypeDescriptor.GetProperties(typeof(BulkMemberUpdateResponseFileData)).OfType<PropertyDescriptor>();

                sb.AppendLine(string.Join(",", fileDescription.Select(p => p.Name)));

                foreach (var membernumber in memberNumberArray)
                {
                    var customerSearchDocument = await _customerSearchDocumentServiceQueryHandler.Handle(request: new CustomerSearchDocumentServiceQuery { MemberNumber = membernumber }, cancellationToken);
                    UpdateVredCustomerDetailsRS updateVredCustomerDetailsRS = new UpdateVredCustomerDetailsRS();

                    var correlationIdForEvents = Guid.NewGuid().ToString();

                    if (customerSearchDocument.IsSuccess && !string.IsNullOrWhiteSpace(request.MemberStatus))
                    {
                        customerDoc = new Customer
                        {
                            CustomerId = customerSearchDocument.Value.Value.CustomerId,
                            LoyaltyMemberNumber = membernumber,
                            FirstName = customerSearchDocument.Value.Value.FirstName,
                            LastName = customerSearchDocument.Value.Value.LastName,
                            LastUpdatedBy = request.InitiatedBy,
                            LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                            EmailId = customerSearchDocument.Value.Value.EmailAddress,
                            Status = request.MemberStatus
                        };

                        flyingClubMemberDoc = new FlyingClubMember
                        {
                            Id = customerSearchDocument.Value.Value.CustomerId,
                            CustomerId = customerSearchDocument.Value.Value.CustomerId,
                            LastUpdatedBy = request.InitiatedBy,
                            Status = request.MemberStatus
                        };

                        try
                        {
                            await Task.WhenAll(
                                               PublishEvent(UpdateBulkEventDataMapper.EventDataMapperCustomer(customerDoc), correlationIdForEvents, Constants.Application.UpdateCustomerEventSubjectinCustomer, cancellationToken),
                                               PublishEvent(BulkTransactionEventRequestMapper.EventDataMapperNotes(customerDoc, request.MemberNotes), correlationIdForEvents, Constants.Application.CustomerTransactioneventmessagesType, cancellationToken),
                                               PublishEvent(BulkTransactionEventRequestMapper.EventDataMapperCustomerTransStatus(flyingClubMemberDoc), correlationIdForEvents, Constants.Application.CustomerTransactionStatuseventmessagesSubject, cancellationToken),
                                               PublishEvent(UpdateBulkEventDataMapper.EventDataMapperB2C(customerDoc), correlationIdForEvents, Constants.Application.UpdateCustomerEventSubjectinB2C, cancellationToken),
                                               PublishEvent(UpdateBulkEventDataMapper.EventDataMapperVRED(customerDoc, correlationIdForEvents), correlationIdForEvents, Constants.Application.UpdateCustomerEventSubjectinVRED, cancellationToken),
                                               PublishEvent(BulkAuditEventRequestMapper.EventDataMapperAudit(customerDoc), correlationIdForEvents, Constants.Application.UpdateCustomerEventSubjectinAudit, cancellationToken)
                                               );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error occurred while publishing cloud events. {ex.Message}");
                            throw;
                        }
                        updateVredCustomerDetailsRS = await vredCustomerDetailsResponseQuery.Handle(request: new UpdateVredCustomerDetailsResponseQuery { CorrelationId = correlationIdForEvents }, cancellationToken);

                        if (updateVredCustomerDetailsRS.UpdateVredCustomerDetailsResponse != null)
                        {
                            sb.AppendLine(string.Join(",",
                                EscapeCsv(membernumber),
                                EscapeCsv(request.MemberNotes),
                                EscapeCsv(request.MemberStatus),
                                EscapeCsv(request.InitiatedBy),
                                EscapeCsv(DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat)),
                                EscapeCsv(updateVredCustomerDetailsRS.UpdateVredCustomerDetailsResponse.ProcessingStatus)
                            ));
                        }
                        else
                        {
                            var errorMessage = updateVredCustomerDetailsRS?.Error?.FirstOrDefault()?.Code
                                ?? "Unknown error";
                            sb.AppendLine(string.Join(",",
                                EscapeCsv(membernumber),
                                EscapeCsv(request.MemberNotes),
                                EscapeCsv(request.MemberStatus),
                                EscapeCsv(request.InitiatedBy),
                                EscapeCsv(DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat)),
                                EscapeCsv(errorMessage)
                            ));
                        }
                    }
                    else if (customerSearchDocument.IsSuccess && string.IsNullOrWhiteSpace(request.MemberStatus) && !String.IsNullOrWhiteSpace(request.MemberNotes))
                    {
                        customerDoc = new Customer
                        {
                            CustomerId = customerSearchDocument.Value.Value.CustomerId,
                            LoyaltyMemberNumber = membernumber,
                            FirstName = customerSearchDocument.Value.Value.FirstName,
                            LastName = customerSearchDocument.Value.Value.LastName,
                            LastUpdatedBy = request.InitiatedBy,
                            LastUpdatedOn = DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat),
                            EmailId = customerSearchDocument.Value.Value.EmailAddress,
                        };
                        try
                        {
                            await PublishEvent(BulkTransactionEventRequestMapper.EventDataMapperNotes(customerDoc, request.MemberNotes), correlationIdForEvents, Constants.Application.CustomerTransactioneventmessagesType, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error occurred while publishing cloud events. {ex.Message}");
                            throw;
                        }

                        sb.AppendLine(string.Join(",",
                               EscapeCsv(membernumber),
                               EscapeCsv(request.MemberNotes),
                               EscapeCsv("NA"),
                               EscapeCsv(request.InitiatedBy),
                               EscapeCsv(DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat)),
                               EscapeCsv("NA")
                        ));
                    }
                    else
                    {
                        sb.AppendLine(string.Join(",",
                            EscapeCsv(membernumber),
                            EscapeCsv(request.MemberNotes),
                            EscapeCsv(request.MemberStatus),
                            EscapeCsv(request.InitiatedBy),
                            EscapeCsv(DateTime.UtcNow.ToString(Constants.Application.EventTimeFormat)),
                            EscapeCsv(customerSearchDocument.Error)
                        ));
                    }
                }

                //Response file Upload to storage
                await _azureStorageCommandHandler.Handle(new AzureStorageCommand()
                {
                    StorageConnection = _StorageUrl,
                    ContainerName = _containerNameForBulkUploadResponse,
                    BlobName = "RS_" + blobName,
                    FileData = sb.ToString(),
                    Action = AzureStorageCommand.Actions.Add
                }, cancellationToken);

                //Archive The Input file
                await _azureStorageCommandHandler.Handle(new AzureStorageCommand()
                {
                    StorageConnection = _StorageUrl,
                    ContainerName = _containerNameForBulkUpload,
                    ArchiveContainer = _archiveContainerNameForBulkUpload,
                    BlobName = blobName,
                    Action = AzureStorageCommand.Actions.Copy
                }, cancellationToken);

                //Archive The Response file
                await _azureStorageCommandHandler.Handle(new AzureStorageCommand()
                {
                    StorageConnection = _StorageUrl,
                    ContainerName = _containerNameForBulkUploadResponse,
                    ArchiveContainer = _archiveContainerNameForBulkUpload,
                    BlobName = "RS_" + blobName,
                    Action = AzureStorageCommand.Actions.Copy
                }, cancellationToken);

                //Delete The input file
                try
                {
                    await _azureBlobService.DeleteBlobAsync(storageAccountUrl, _containerNameForBulkUpload, _sasKey, blobName, cancellationToken);
                }
                catch (Azure.RequestFailedException ex) when (ex.Status == 404)
                {
                    return Result.Failure<Response<BulkMemberUpdateResponses>>(ex.ErrorCode);
                }

            }
            catch (System.Exception ex)
            {
                return Result.Failure<Response<BulkMemberUpdateResponses>>(ex.Message);
            }

            return new Response<BulkMemberUpdateResponses>
            {
                Status = Status.OK,
                Value = new BulkMemberUpdateResponses
                {
                    Message = "BulkUpload Processed Successfully",
                    Status = "OK",
                    Error = null
                }
            };
        }

        private static string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            // Neutralize formula injection
            if ("=+-@".Contains(value[0]))
                value = "'" + value;

            // Escape quotes
            string safe = value.Replace("\"", "\"\"");

            // Quote if necessary
            bool mustQuote = value.Contains(",") || value.Contains("\"") || value.Contains("\n");
            return mustQuote ? $"\"{safe}\"" : safe;
        }


        private async Task PublishEvent(CloudEvent bulkUpdateStatusEvent, string correlationIdForEvents, string type, CancellationToken cancellationToken)
        {

            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = type, Data = bulkUpdateStatusEvent.Data, Subject = bulkUpdateStatusEvent.Subject }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand { Message = bulkUpdateStatusEvent, ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString), ServiceBusTopicName = bulkUpdateStatusEvent.Source }, cancellationToken);
            }

            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = type, Data = bulkUpdateStatusEvent.Data, Subject = bulkUpdateStatusEvent.Subject }, cancellationToken);
                throw;
            }
        }
    }
}
