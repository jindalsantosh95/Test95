using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.Account;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Interfaces;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{

    public class UpdateMemberStatusCommandHandler : IUpdateMemberStatusCommand
    {
        private readonly ILogger<UpdateMemberStatusCommandHandler> _logger;
        private readonly CustomerRepoUpdateCommandHandler _customerRepoUpdateCommandHandler;
        private readonly CustomerSearchQueryHandler _customerSearchQueryHandler;
        private readonly CustomerDucomentQueryHandler _customerDucomentQueryHandler;
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler;
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler;
        private readonly ServiceBusTopicMessageEventHandler _serviceBusTopicMessageEventHandler;

        public UpdateMemberStatusCommandHandler(ILogger<UpdateMemberStatusCommandHandler> logger, CustomerSearchQueryHandler customerSearchQueryHandler, CustomerDucomentQueryHandler customerDucomentQueryHandler, CustomerRepoUpdateCommandHandler customerRepoUpdateCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ServiceBusTopicMessageEventHandler serviceBusTopicMessageEventHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerSearchQueryHandler = customerSearchQueryHandler ?? throw new ArgumentNullException(nameof(customerSearchQueryHandler));
            _customerDucomentQueryHandler = customerDucomentQueryHandler ?? throw new ArgumentNullException(nameof(customerDucomentQueryHandler));
            _customerRepoUpdateCommandHandler = customerRepoUpdateCommandHandler ?? throw new ArgumentNullException(nameof(customerRepoUpdateCommandHandler));
            _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
            _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
            _serviceBusTopicMessageEventHandler = serviceBusTopicMessageEventHandler ?? throw new AbandonedMutexException(nameof(serviceBusTopicMessageEventHandler));
        }

        public async Task<Result<Response<string>>> Handle(BulkMemberStatusUpdateRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.MemberNumber == null && string.IsNullOrWhiteSpace(request.MemberStatus))
                {
                    throw new ArgumentNullException(nameof(request));
                }
                foreach (var memberNumber in request.MemberNumber)
                {
                    CustomerSearch existingDocCustomerSearch = (await _customerSearchQueryHandler.Handle(new CustomerSearchQuery(memberNumber), cancellationToken)).Value.Value;

                    Customers customerDoc = (await _customerDucomentQueryHandler.Handle(new CustomerDucomentQuery(memberNumber), cancellationToken)).Value.Value;
                    var updateCustomerDoc = CustomerDocumentUpdateMapper.BulkUpdateMemberStatusMapper(customerDoc, request.MemberStatus);

                    var updatedCustomerDoc = (await _customerRepoUpdateCommandHandler.Handle(new CustomerRepoUpdateCommand(updateCustomerDoc), cancellationToken));

                    if (updatedCustomerDoc.IsSuccess)
                    {
                        //publish event for B2C and VRED email update

                        var correlationIdForEvents = Guid.NewGuid().ToString();
                        try
                        {
                            await Task.WhenAll(
                                               PublishEvent(UpdateEventDataMapper.EventDataMapperVRED(updateCustomerDoc), correlationIdForEvents, Constants.Application.UpdateStatusVREDEventType, cancellationToken),
                                               PublishEvent(UpdateEventDataMapper.EventDataMapperB2C(existingDocCustomerSearch, request.MemberStatus), correlationIdForEvents, Constants.Application.UpdateStatusB2CEventType, cancellationToken));

                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error occurred while publishing cloud events. {ex.Message}");
                            return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
                        }
                    }

                }
                return new Response<string>()
                {
                    Status = Status.OK,
                    Value = "BulkUpdate Member Status Successful",
                    Errors = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error occured while updating MemberStatus");
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private async Task PublishEvent(CloudEvent bulkUpdateStatusEvent, string correlationIdForEvents, string type, CancellationToken cancellationToken)
        {

            await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = type, Data = bulkUpdateStatusEvent.Data, Subject = bulkUpdateStatusEvent.Subject }, cancellationToken);

            try
            {
                await _serviceBusTopicMessageEventHandler.Handle(bulkUpdateStatusEvent, cancellationToken);
            }

            catch
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = type, Data = bulkUpdateStatusEvent.Data, Subject = bulkUpdateStatusEvent.Subject }, cancellationToken);
                throw;
            }

        }

        private static Response<string> GetErrorResponse(string errorCode)
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };

        }
    }
}
