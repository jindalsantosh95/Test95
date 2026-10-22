using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Validation;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.AgencyValidate;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Infrastructure.Repositories.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Mapper;
using VAA.EAPI.Common.Extensions;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Application.Query.AgencyValidate
{
    public class AgencyValidateQueryHandler : IRequestHandler<AgencyValidateQuery, Result<Response<AgencyValidateResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly IAgencyDataRepository _agencyDataRepository;
        private readonly ILogger<AgencyValidateQueryHandler> _logger;

        public AgencyValidateQueryHandler(IMediator mediator, IAgencyDataRepository agencyDataRepository, ILogger<AgencyValidateQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _agencyDataRepository = agencyDataRepository ?? throw new ArgumentNullException(nameof(agencyDataRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<AgencyValidateResponse>>> Handle(AgencyValidateQuery requestData, CancellationToken cancellationToken)
        {
            var requestResult = ValidateRequest.RequestValidator(requestData);

            if (requestResult.Value.Status == Status.BusinessException)
            {
                return Result.Success(new Response<AgencyValidateResponse>
                {
                    Errors = requestResult.Value.Errors,
                    Status = Status.BusinessException
                });
            }

            var agencyDetails = await _agencyDataRepository.GetAgencyDetails(requestData);

            if ((agencyDetails?.Any() ?? false) && requestData.checkAgentTktAuthority
                && agencyDetails.First().RequestAgencyIdType.Equals(Constants.Application.IATA, StringComparison.CurrentCultureIgnoreCase))
            {
                var agentTicketingAuthorityresult = ValidateAgentTicketingAuthority(requestData, agencyDetails);

                if (agentTicketingAuthorityresult.Result.Value?.Status == Status.BusinessException)
                {
                    return Result.Success(new Response<AgencyValidateResponse>
                    {
                        Errors = agentTicketingAuthorityresult.Result.Value?.Errors,
                        Status = Status.BusinessException
                    });
                }
            }

            var result = ValidateRequest.AgencyDetailsValidator(requestData, agencyDetails);

            if (result.Value.Status == Status.BusinessException)
            {
                return Result.Success(new Response<AgencyValidateResponse>
                {
                    Errors = result.Value.Errors,
                    Status = Status.BusinessException
                });
            }

            //Call SecurityMS
            if (ConfigReader.Get<bool>(Constants.Environment.CallSecuritySvcFlag) && !requestData.OCNValidation)
            {
                var sellerId = requestData?.Parties?.Participants?
                    .FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Seller)?.AgencyID;
                var aggregatorId = requestData?.Parties?.Participants?
                    .FirstOrDefault(participant => participant.PartyType == Participant.PartyTypeEnum.Distributor)?.AgencyID;
                var isAggregatorRequest = !aggregatorId.IsNullOrEmpty();
                var agencyID = isAggregatorRequest ? $"{sellerId}-{aggregatorId}" : sellerId;

                var retrieveSubscriptionResponse = await _mediator.Send(new SecuritySvcRetrieveQuery { AgencyID = agencyID, Environment = requestData.Environment }, cancellationToken);

                // Security MS may not have a subscription queue for the agent/aggregator pair.
                // The NDConnect mapping has already validated the aggregator, so retry using
                // the aggregator-only subscription before rejecting the request.
                if (isAggregatorRequest && (IsUnavailableSubscription(retrieveSubscriptionResponse)
                    || IsSubscriptionKeyMismatch(requestData, retrieveSubscriptionResponse)))
                {
                    retrieveSubscriptionResponse = await _mediator.Send(
                        new SecuritySvcRetrieveQuery { AgencyID = aggregatorId, Environment = requestData.Environment },
                        cancellationToken);
                }

                var subscriptionResult = ValidateRequest.SubscriptionDetailsValidator(requestData, retrieveSubscriptionResponse);

                if (subscriptionResult.Value.Status == Status.BusinessException)
                {
                    return Result.Success(new Response<AgencyValidateResponse>
                    {
                        Errors = subscriptionResult.Value.Errors,
                        Status = Status.BusinessException
                    });
                }
            }

            return new Response<AgencyValidateResponse>
            {
                Status = Status.OK,
                Value = AgencyValidateResponseMapper.MapAgencyDetails(requestData,agencyDetails)
            };
        }

        private static bool IsUnavailableSubscription(SubscriptionResponse? subscriptionResponse)
        {
            var subscriptionKey = subscriptionResponse?.SubscriptionKeys?.FirstOrDefault()?.Key;

            return subscriptionResponse == null
                || subscriptionResponse.Error != null
                || string.IsNullOrWhiteSpace(subscriptionKey);
        }

        private static bool IsSubscriptionKeyMismatch(AgencyValidateQuery requestData, SubscriptionResponse? subscriptionResponse)
        {
            var subscriptionKey = subscriptionResponse?.SubscriptionKeys?.FirstOrDefault()?.Key;

            return subscriptionResponse?.Error == null
                && subscriptionResponse?.SubscriptionState != null
                && subscriptionResponse.SubscriptionState.Is(Constants.Application.EnabledStatus)
                && !subscriptionKey.IsNullOrWhiteSpace()
                && !string.Equals(subscriptionKey, requestData.AgentSubscriptionKey, StringComparison.Ordinal);
        }

        private async Task<Result<Response<string>>> ValidateAgentTicketingAuthority(AgencyValidateQuery requestData, List<AgencyData> agencyDetails)
        {
            var results = new Result<Response<string>>();
            var agentExpiryDuration = ConfigReader.Get<TimeSpan>(Constants.Environment.AgentTktAuthorityCacheExpiryDuration);
            DateTime agentExpiryDate = DateTime.Now.Subtract(agentExpiryDuration);

            var sellerAgencyId = requestData.Parties.Participants.First(x => x.PartyType == Participant.PartyTypeEnum.Seller).AgencyID;
            var sellerDetails = agencyDetails.FirstOrDefault(x => x.RequestAgencyId == sellerAgencyId);

            requestData.RequestAgencyType = sellerDetails?.RequestAgencyIdType ?? string.Empty;

            DateTime lastUpdateIATA = sellerDetails?.LastUpdateIATA ?? DateTime.MinValue;
            if (agentExpiryDate > lastUpdateIATA)
            {
                var retrieveTktAuthResponse = await _mediator.Send(new AgentTktAuthRetrieveQuery { RequestData = requestData });
                results = ValidateRequest.AgentTicketingAuthorityValidation(requestData, retrieveTktAuthResponse, agencyDetails);
            }

            return results;
        }
    }
}
