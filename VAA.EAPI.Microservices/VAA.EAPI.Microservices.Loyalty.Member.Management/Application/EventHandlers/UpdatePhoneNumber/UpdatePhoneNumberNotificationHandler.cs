using CSharpFunctionalExtensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers.UpdatePhoneNumber
{
    public class UpdatePhoneNumberNotificationHandler(GroupDocumentServiceQueryHandler groupDocumentServiceQueryHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventHandler) : IUpdatePhoneNumberCommand
    {
        private readonly GroupDocumentServiceQueryHandler _groupDocumentServiceQueryHandler = groupDocumentServiceQueryHandler ?? throw new ArgumentNullException(nameof(groupDocumentServiceQueryHandler));
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventHandler));
        public async Task<Result<Response<string>>> Handle(UpdatePhoneNumberNotificationEventRequest request, CancellationToken cancellationToken)
        {
            ValidateRequest(request);

            var groupDocumentResult = await GetGroupDocumentByHoHCustomerId(request.HOHCustomerId, cancellationToken);

            if (groupDocumentResult.IsFailure)
            {
                return Result.Failure<Response<string>>(

                groupDocumentResult.Error);
            }

            var groupDetails = groupDocumentResult.Value.Value;

            if (groupDetails == null || groupDetails.GroupHistory == null || groupDetails.GroupHistory.Count <= 0)
            {
                return Result.Failure<Response<string>>("Invalid Request - no group history found");
            }


            var activeManagedMembers = groupDetails.GroupHistory
                 .Where(history =>
                     !history.ExitDate.HasValue &&
                     history.HoHManaged == true)
                 .ToList();

            if (activeManagedMembers.Count == 0)
            {
                return Result.Failure<Response<string>>(
                    "Invalid request - no active child HoH-managed members found.");
            }

            var publishTasks = activeManagedMembers.Select(history =>
                PublishUpdatePhoneNumberNotificationEvent(
                    history.CustomerId,
                    request,
                    cancellationToken));

            var publishResults = await Task.WhenAll(publishTasks);

            var failure = publishResults.FirstOrDefault(x => x.IsFailure);

            if (failure.IsFailure)
            {
                return Result.Failure<Response<string>>(failure.Error);
            }

            return new Response<string>
            {
                Status = Status.OK,
                Value = "Request processed successfully"
            };
        }

        private void ValidateRequest(UpdatePhoneNumberNotificationEventRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.HOHCustomerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.GroupID);
            ArgumentNullException.ThrowIfNull(request.Phone);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.Phone.PhoneNumber);
            ArgumentException.ThrowIfNullOrWhiteSpace(request.InitiatedBy);
        }

        private async Task<Result<Response<HouseHoldGroup>>> GetGroupDocumentByHoHCustomerId(string hohcustomerId, CancellationToken cancellationToken)
        {
            var response = await _groupDocumentServiceQueryHandler.Handle(request: new GroupDocumentServiceQuery { CustomerId = hohcustomerId }, cancellationToken);

            return response.IsSuccess ? Result.Success(response.Value) : Result.Failure<Response<HouseHoldGroup>>(error: $"Failed to retrieve group document. {Constants.ErrorCodes.InvalidCustomerId}");
        }
        private async Task<Result<Response<string>>> PublishUpdatePhoneNumberNotificationEvent(string childcustomerId, UpdatePhoneNumberNotificationEventRequest updatePhoneNumberNotificationEventRequest, CancellationToken cancellationToken)
        {
            UpdatePhoneNumberEventData updatePhoneNumberEventData = new UpdatePhoneNumberEventData()
            {
                CustomerId = childcustomerId,
                PhoneNumber = updatePhoneNumberNotificationEventRequest.Phone.PhoneNumber,
                CountryCode = updatePhoneNumberNotificationEventRequest.Phone.CountryCode,
                FcNumber = updatePhoneNumberNotificationEventRequest.FcNumber,
                CorrelationId = Guid.NewGuid().ToString(),
            };

            var cloudEvent = Mapper.CustomerProfile.UpdatePhoneNumberNotificationEventToCloudEventMapper.CommandToEventMapper(updatePhoneNumberEventData);

            try
            {
                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand
                {
                    Message = cloudEvent,
                    ServiceBusConnectionString = ConfigReader.Get(Constants.Environment.ServiceBusConnectionString),
                    ServiceBusTopicName = cloudEvent.Source
                }, cancellationToken);

                return new Response<string>
                {
                    Status = Status.OK,
                    Value = "Request processed successfully"
                };
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

    }
}
