using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Customer;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.EmailUpdate;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Mapper.GroupDoc;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers.EmailUpdate
{
    public class EmailChangeNotificationEventHandler(FlyingClubMemberDocumentServiceQueryHandler flyingClubMemberDocumentServiceQueryHandler, GroupDocumentServiceQueryHandler groupDocumentServiceQueryHandler, ICustomerProvider customerProvider, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventHandler) : IEmailChangeNotificationCommand
    {
        private readonly FlyingClubMemberDocumentServiceQueryHandler _flyingClubMemberDocumentServiceQueryHandler = flyingClubMemberDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(flyingClubMemberDocumentServiceQueryHandler));
        private readonly GroupDocumentServiceQueryHandler _groupDocumentServiceQueryHandler = groupDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(groupDocumentServiceQueryHandler));
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventHandler));
        private readonly ICustomerProvider _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));   
        public async Task<Result<Response<string>>> Handle(EmailChangeEvent emailChangeRequest, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(emailChangeRequest);

            ArgumentException.ThrowIfNullOrWhiteSpace(emailChangeRequest.CustomerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailChangeRequest.MemberId);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailChangeRequest.OldEmailAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailChangeRequest.NewEmailAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailChangeRequest.InitiatedBy);

            var flyingClubMemberDoc = await GetFlyingClubMemberDocument(emailChangeRequest.CustomerId, cancellationToken);

            if (flyingClubMemberDoc is null)
            {
                return Result.Failure<Response<String>>(error: $"Invalid customer id :{Constants.ErrorCodes.InvalidCustomerId}");
            }

            if (flyingClubMemberDoc.Status != Constants.Application.FCMemberActiveStatus)
            {
                return Result.Failure<Response<String>>($"EmailChangeNotificationEventHandler - Head of the account status is not active.ErrorCode:{Constants.ErrorCodes.FCMemberAccountNotActive}");
            }
            CustomerSearch hohcustomerSearchDoc = await _customerProvider.GetCustomerSearchDocument(flyingClubMemberDoc.HeadOfGroupMemberNumber);
           
            if (hohcustomerSearchDoc is null)
            {
                return Result.Failure<Response<String>>(error: $"Invalid customer id :{Constants.ErrorCodes.InvalidCustomerId}");
            }
           
            HouseHoldGroup groupDocByHoHCustId = (await GetGroupDocumentByHoHCustomerId(hohcustomerSearchDoc.CustomerId, cancellationToken)).Value?.Value;

            if (groupDocByHoHCustId is null)
            {
                return Result.Failure<Response<String>>($"Invalid HeadOfGroupMemberNumber. {Constants.ErrorCodes.InvalidCustomerId}");
            }
            if (string.IsNullOrWhiteSpace(groupDocByHoHCustId.GroupId))
            {
                return Result.Failure<Response<String>>($"Invalid HeadOfGroupMemberNumber. {Constants.ErrorCodes.InvalidCustomerId}");
            }
            var result = await UpdateGroupDoc(groupDocByHoHCustId.GroupId, emailChangeRequest, groupDocByHoHCustId, cancellationToken);

            if (!result.IsSuccess)
            {
                return Result.Failure<Response<string>>(error: $"Failed to update group document. Error: {result.Error}");
            }
            return Result.Success(new Response<string> { Value = result.Value?.Value });
        }
        private async Task<Result<Response<string>>> UpdateGroupDoc(string groupDocId, EmailChangeEvent emailChangeRequest, HouseHoldGroup groupDoc, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(groupDocId);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailChangeRequest.CustomerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(emailChangeRequest.InitiatedBy);
            if (groupDoc is null)
            {
                throw new ArgumentNullException(nameof(groupDoc));
            }

            var groupHistoryResponse = groupDoc.GroupHistory?
                .LastOrDefault(history =>
                    history.ExitDate == null &&
                    history.CustomerId == emailChangeRequest.CustomerId);

            if (groupHistoryResponse is null)
            {
                return Result.Failure<Response<string>>(
                    $"Failed to update group document. No active group history found for customer '{emailChangeRequest.CustomerId}'.");
            }

            GroupHistory groupHistory = new()
            {
                HistoryId = groupHistoryResponse.HistoryId,
                CreatedBy = groupHistoryResponse.CreatedBy,
                CreatedOn = groupHistoryResponse.CreatedOn,
                LastUpdatedBy = emailChangeRequest.InitiatedBy,
                LastUpdatedOn = DateTime.UtcNow,
                StartDate = groupHistoryResponse.StartDate,
                ExitDate = null,
                HoHManaged = emailChangeRequest.NewEmailAddress.EndsWith(ConfigReader.Get(Constants.Application.DummyEmailDomain), StringComparison.OrdinalIgnoreCase) ? true : false,
                CustomerId = groupHistoryResponse.CustomerId
            };

            HouseHoldGroup updateGroup = new()
            {
                GroupId = groupDocId,
                LastUpdatedBy = emailChangeRequest.InitiatedBy,
                LastUpdatedOn = DateTime.UtcNow,
                GroupHistory = new List<GroupHistory> { groupHistory }
            };

            HouseHoldGroupUpdateEvent updateEvent = new()
            {
                GroupDoc = updateGroup
            };

            var cloudEvent = UpdateGroupDocEventToCloudEventMapper.CommandToEventMapper(updateEvent);

            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(
                    new EventRequestInsertCommand
                    {
                        Id = cloudEvent.Id,
                        Type = cloudEvent.Type,
                        Data = cloudEvent.Data,
                        Subject = cloudEvent.Subject
                    },
                    cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(
                    new ServiceBusTopicMessageEventCommand
                    {
                        Message = cloudEvent,
                        ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                        ServiceBusTopicName = cloudEvent.Source
                    },
                    cancellationToken);

                return Result.Success(
                    new Response<string>
                    {
                        Value = cloudEvent.Id
                    });
            }
            catch (Exception ex)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(
                    new EventStoreRequestDeleteCommand
                    {
                        Id = cloudEvent.Id,
                        Type = cloudEvent.Type,
                        Data = cloudEvent.Data,
                        Subject = cloudEvent.Subject
                    },
                    cancellationToken);

                return Result.Failure<Response<string>>(
                    $"Failed to update group document. Error: {ex.Message}");
            }
        }
        private async Task<FlyingClubMember> GetFlyingClubMemberDocument(string customerId, CancellationToken cancellationToken)
        {
            var response = await _flyingClubMemberDocumentServiceQueryHandler.Handle(new FlyingClubMemberDocumentServiceQuery
            {
                CustomerId = customerId
            }, cancellationToken);

            return response.IsSuccess ? response.Value.Value : null;
        }
        private async Task<Result<Response<HouseHoldGroup>>> GetGroupDocumentByHoHCustomerId(string customerId, CancellationToken cancellationToken)
        {
            var response = await _groupDocumentServiceQueryHandler.Handle(request: new GroupDocumentServiceQuery { CustomerId = customerId }, cancellationToken);

            return response.IsSuccess ? Result.Success(response.Value) : Result.Failure<Response<HouseHoldGroup>>(error: $"Failed to retrieve group document. {Constants.ErrorCodes.InvalidCustomerId}");
        }
    }
}
