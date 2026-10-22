using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount
{
    public class CustomerProfileUpdatePreferencesCommandHandler(CustomerProfileQueryHandler customerProfileQueryHandler, CosmosModifyServiceCommandHandler cosmosModifyServiceCommandHandler, EventStoreRequestInsertCommandHandler eventStoreRequestInsertCommandHandler, EventStoreRequestDeleteCommandHandler eventStoreRequestDeleteCommandHandler, ILogger<CustomerProfileUpdatePreferencesCommandHandler> logger, ServiceBusTopicMessageEventCommandHandler serviceBusTopicMessageEventCommandHandler) : ICustomerProfileUpdatePreferencesCommand
    {
        private readonly CustomerProfileQueryHandler _customerProfileQueryHandler = customerProfileQueryHandler ?? throw new ArgumentNullException(nameof(customerProfileQueryHandler));
        private readonly CosmosModifyServiceCommandHandler _cosmosModifyServiceCommandHandler = cosmosModifyServiceCommandHandler ?? throw new ArgumentNullException(nameof(cosmosModifyServiceCommandHandler));
        private readonly EventStoreRequestInsertCommandHandler _eventStoreRequestInsertCommandHandler = eventStoreRequestInsertCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestInsertCommandHandler));
        private readonly EventStoreRequestDeleteCommandHandler _eventStoreRequestDeleteCommandHandler = eventStoreRequestDeleteCommandHandler ?? throw new ArgumentNullException(nameof(eventStoreRequestDeleteCommandHandler));
        private readonly ILogger<CustomerProfileUpdatePreferencesCommandHandler> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ServiceBusTopicMessageEventCommandHandler _serviceBusTopicMessageEventCommandHandler = serviceBusTopicMessageEventCommandHandler ?? throw new ArgumentNullException(nameof(serviceBusTopicMessageEventCommandHandler));

        public async Task<UpdateMarketingPreferenceResponse> Handle(CustomerProfileCommand request, CancellationToken cancellationToken)
        {
            if (request == null || request.Preference == null)
            {
                _logger.LogError("Failed to update customer profile due to missing preferences for CustomerId: {CustomerId}", request?.CustomerId);
                return new UpdateMarketingPreferenceResponse()
                {
                    PreferenceValue = false,
                    ErrorDetails = EAPI.Infrastructure.Helper.ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }

            var preferredPartnerFfp = request.Preference.FirstOrDefault(p => p.PreferenceType == PreferenceTypes.PreferredPartnerFFP);
            if (preferredPartnerFfp?.PreferenceValue?.Length > 3)
            {
                _logger.LogError("Failed to update customer profile for CustomerId: {CustomerId}", request.CustomerId);
                return new UpdateMarketingPreferenceResponse()
                {
                    PreferenceValue = false,
                    ErrorDetails = EAPI.Infrastructure.Helper.ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidPreferredPartnerFFP), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }

            var customerProfileResponse = await _customerProfileQueryHandler.Handle(new CustomerProfileQuery { CustomerId = request.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.CustomerProfileDocType) }, cancellationToken);

            if (!customerProfileResponse.IsSuccess)
            {
                _logger.LogError("Failed to retrieve customer profile for CustomerId: {CustomerId}", request.CustomerId);
                return new UpdateMarketingPreferenceResponse()
                {
                    PreferenceValue = false,
                    ErrorDetails = EAPI.Infrastructure.Helper.ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }

            if (customerProfileResponse.Value.Value.CustomerProfile != null)
            {
                var responseMapper = CustomerProfileUpdatePreferenceMapper.UpdatePreferenceMapper(customerProfileResponse.Value.Value.CustomerProfile, request);
                await _cosmosModifyServiceCommandHandler.Handle(UpdateRequestServiceCommand(responseMapper), cancellationToken);

                await UpdatePreferenceEvent(request, responseMapper, cancellationToken);

                if (request.Preference.Any(p => p.PreferenceType == PreferenceTypes.VAAMarketingPermissions || p.PreferenceType == PreferenceTypes.VAPartnersMarketingPermissions))
                    await UpdatePreferenceEmailEvent(request, cancellationToken);

                return new UpdateMarketingPreferenceResponse()
                {
                    PreferenceValue = true
                };
            }
            else
            {
                _logger.LogError("Customer profile document not found for CustomerId: {CustomerId}", request.CustomerId);
                return new UpdateMarketingPreferenceResponse()
                {
                    PreferenceValue = false,
                    ErrorDetails = EAPI.Infrastructure.Helper.ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MiscellaneousTechnicalError), AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }

        public static CosmosModifyServiceCommand UpdateRequestServiceCommand(CustomerProfileDTO updatedCustomerProfileDoc)
        {
            return new CosmosModifyServiceCommand()
            {
                CollectionName = ConfigReader.Get(EnvironmentSettings.TransactionDBCollection),
                DatabaseEndpoint = ConfigReader.Get(EnvironmentSettings.TransactionDBEndPoint),
                DatabaseName = ConfigReader.Get(EnvironmentSettings.TransactionDB),
                Event = new CosmosDocumentEvent()
                {
                    Document = updatedCustomerProfileDoc,
                    ModificationType = CosmosModificationType.Replace
                }
            };
        }

        private async Task UpdatePreferenceEvent(CustomerProfileCommand request, CustomerProfileDTO customerProfileResponse, CancellationToken cancellationToken)
        {
            CloudEvent eventMessage = new();
            var correlationIdForEvents = AsyncContext.Current.CorrelationId.ToString();

            try
            {
                eventMessage = AuditUpdatePreferenceRequestMapper.EventDataMapper(correlationIdForEvents, request, customerProfileResponse);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.LoyaltyAuditPreferenceEventType, Data = eventMessage.Data, Subject = Constants.Application.LoyaltyAuditPreferenceSubject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand() { Message = eventMessage, ServiceBusConnectionString = ConfigReader.Get(EnvironmentSettings.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.loyaltyAuditPreferenceSource }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.LoyaltyAuditPreferenceEventType, Data = eventMessage.Data, Subject = Constants.Application.LoyaltyAuditPreferenceSubject }, cancellationToken);
                _logger.LogError(ex, "Error occurred while publishing audit preference update event");
            }
        }

        private async Task UpdatePreferenceEmailEvent(CustomerProfileCommand request, CancellationToken cancellationToken)
        {
            var correlationIdForEvents = AsyncContext.Current.CorrelationId.ToString();
            CloudEvent eventMessage = new();

            try
            {
                eventMessage = AuditUpdatePreferenceRequestMapper.EmailEventDataMapper(correlationIdForEvents, request);

                await _eventStoreRequestInsertCommandHandler.Handle(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateCustomerEventSubject, Data = eventMessage.Data, Subject = Constants.Application.UpdateCustomerEventSubject }, cancellationToken);

                await _serviceBusTopicMessageEventCommandHandler.Handle(new ServiceBusTopicMessageEventCommand() { Message = eventMessage, ServiceBusConnectionString = ConfigReader.Get(EnvironmentSettings.ServiceBusConnectionString), ServiceBusTopicName = Constants.Application.ManagaeAccountUpdateTopicName }, cancellationToken);
            }
            catch (Exception ex)
            {
                await _eventStoreRequestDeleteCommandHandler.Handle(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateCustomerEventSubject, Data = eventMessage.Data, Subject = Constants.Application.UpdateCustomerEventSubject }, cancellationToken);
                _logger.LogError(ex, "Error occurred while publishing email preference event");
            }
        }
    }
}
