using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Mapper.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Applications.Query.APIMSubscription
{
    public class SubscriptionResponseQueryHandler : IRequestHandler<SubscriptionResponseQuery, Result<Response<SubscriptionResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionResponseQueryHandler> _logger;

        public SubscriptionResponseQueryHandler(IMediator mediator, ILogger<SubscriptionResponseQueryHandler> logger)
        {
            _mediator = mediator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<SubscriptionResponse>>> Handle(SubscriptionResponseQuery request, CancellationToken cancellationToken)
        {

            Response<SubscriptionResponse> subscriptionApiResponse = new();
            try
            {
                int retryCounter = 1;
                int responseDelayTimeInMS = Convert.ToInt32(ConfigReader.Get(Constants.Environment.ResponseDelayTimeInMS));
                while (retryCounter <= Convert.ToInt32(ConfigReader.Get(Constants.Environment.ResponsePollingCount)))

                {
                    var result = await _mediator.Send(new ServiceResponseQuery { CorrelationId = request.CorrelationId, EventType = request.Type });
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS);
                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:
                            await Task.Delay(timespan, cancellationToken);
                            break;
                        case ServiceResponseProcessStatus.Completed:
                            subscriptionApiResponse = SubscriptionApiResponseMapping.SubscriptionApiResponseMapper(result);
                            return subscriptionApiResponse.Value != null ? Result.Success(subscriptionApiResponse) : Result.Failure<Response<SubscriptionResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                        case ServiceResponseProcessStatus.Error:
                            return Result.Failure<Response<SubscriptionResponse>>(result.JsonResponseData);
                    }
                    retryCounter++;
                }
                return null;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Subscription response lookup failed.");
                return Result.Failure<Response<SubscriptionResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
