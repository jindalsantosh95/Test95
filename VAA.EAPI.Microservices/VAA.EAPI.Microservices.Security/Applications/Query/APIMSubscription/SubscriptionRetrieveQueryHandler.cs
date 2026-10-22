using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription;


namespace VAA.EAPI.Microservices.Security.Applications.Query.APIMSubscription
{
    public class SubscriptionRetrieveQueryHandler : IRequestHandler<SubscriptionRetrieveQuery, Result<Response<SubscriptionResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SubscriptionRetrieveQueryHandler> _logger;

        public SubscriptionRetrieveQueryHandler(IMediator mediator, ILogger<SubscriptionRetrieveQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<SubscriptionResponse>>> Handle(SubscriptionRetrieveQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var subscriptionRetrieveResponse = await _mediator.Send(new RetrieveAPIMSubscriptionRepoQuery(request), cancellationToken);

                if (subscriptionRetrieveResponse.Status == Status.OK)
                {
                    return new Response<SubscriptionResponse>()
                    {
                        Status = Status.OK,
                        Value = subscriptionRetrieveResponse
                    };
                }
                else
                {
                    return Result.Failure<Response<SubscriptionResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SubscriptionRetrieveQueryHandler.Handle failed for {SubscriptionId}.", request.SubscriptionId);

                return Result.Failure<Response<SubscriptionResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
