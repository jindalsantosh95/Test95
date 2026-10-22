using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CustomerProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Exceptions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper;
using VAA.EAPI.Common.Helpers;


namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.ManageAccount
{

    public class CustomerProfileCommandHandler : IRequestHandler<CustomerProfileCommand, UpdateMarketingPreferenceResponse>
    {

        private readonly ILogger<CustomerProfileCommandHandler> _logger;
        private readonly IMediator _mediator;

        public CustomerProfileCommandHandler(ILogger<CustomerProfileCommandHandler> logger, IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<UpdateMarketingPreferenceResponse> Handle(CustomerProfileCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                string marketingPrefernceValue = GetMarketingPreference(request.Preference);

                if (string.IsNullOrEmpty(marketingPrefernceValue))
                {
                    return GetErrorResponse(ErrorCodes.InCorrectPreferenceValue);
                }

                var customerProfileResponse = _mediator.Send(new CustomerProfileQuery { CustomerId = request.CustomerId, DocType = ConfigReader.Get(EnvironmentSettings.CustomerProfileDocType) }, cancellationToken);

                if (customerProfileResponse.Result.Value.Value.CustomerProfile != null)
                {
                    var responseMapper = CustomerProfileUpdateReferenceMapper.UpdatePreferenceMapper(customerProfileResponse.Result.Value.Value.CustomerProfile, request);
                    var response = await _mediator.Send(UpdateRequestServiceCommand(responseMapper), cancellationToken);
                    await UpdatePreferenceEvent(request, responseMapper, cancellationToken);
                    await UpdatePreferenceEmailEvent(request, cancellationToken);

                }
                return new UpdateMarketingPreferenceResponse()
                {
                    PreferenceValue = true
                };

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "PreferenceUpdatefailed");
                return GetErrorResponse(ErrorCodes.MiscellaneousTechnicalError);
            }

        }

        private async Task UpdatePreferenceEvent(CustomerProfileCommand request, CustomerProfileDTO customerProfileResponse, CancellationToken cancellationToken)
        {
            var correlationIdForEvents = Guid.NewGuid().ToString();
            var eventmessage = AuditUpdatePreferenceRequestMapper.EventDataMapper(correlationIdForEvents, request, customerProfileResponse);

            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.LoyaltyAuditPreferenceEventType, Data = eventmessage.Data, Subject = Constants.Application.LoyaltyAuditPreferenceSubject }, cancellationToken);
            try
            {
                await _mediator.Publish(eventmessage, cancellationToken);
            }
            catch (Exception ex)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.LoyaltyAuditPreferenceEventType, Data = eventmessage.Data, Subject = Constants.Application.LoyaltyAuditPreferenceSubject }, cancellationToken);
                _logger.LogError(ex, "Error occurred while publishing audit preference update events");
            }
        }

        private async Task UpdatePreferenceEmailEvent(CustomerProfileCommand request, CancellationToken cancellationToken)
        {
            var correlationIdForEvents = Guid.NewGuid().ToString();
            var eventmessage = AuditUpdatePreferenceRequestMapper.EmailEventDataMapper(correlationIdForEvents, request);

            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateCustomerEventSubject, Data = eventmessage.Data, Subject = Constants.Application.UpdateCustomerEventSubject }, cancellationToken);
            try
            {
                await _mediator.Publish(eventmessage, cancellationToken);
            }
            catch (Exception ex)
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.UpdateCustomerEventSubject, Data = eventmessage.Data, Subject = Constants.Application.UpdateCustomerEventSubject }, cancellationToken);
                _logger.LogError(ex, "Error occurred while publishing audit preference update events");
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

        private static UpdateMarketingPreferenceResponse GetErrorResponse(string errorCode)
        {
            return new UpdateMarketingPreferenceResponse()
            {
                ErrorDetails = new ErrorDetails[]
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };

        }

        public static string GetMarketingPreference(IEnumerable<Domain.ManageAccount.Preference> preferences)
        {
            var preferenceValue = string.Empty;

            switch (preferences?.FirstOrDefault().PreferenceValue)
            {
                case Constants.Application.preferenceEnabled:
                    preferenceValue = Constants.Application.preferenceEnabled;
                    break;
                case Constants.Application.preferenceDisabled:
                    preferenceValue = Constants.Application.preferenceDisabled;
                    break;
                default:
                    return string.Empty;
            }

            return preferenceValue;
        }
    }
}
