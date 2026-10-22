using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class RetrieveAPIMSubscriptionRepoQueryHandler : IRequestHandler<RetrieveAPIMSubscriptionRepoQuery, SubscriptionResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RetrieveAPIMSubscriptionRepoQueryHandler> _logger;

        public RetrieveAPIMSubscriptionRepoQueryHandler(IMediator mediator, ILogger<RetrieveAPIMSubscriptionRepoQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SubscriptionResponse> Handle(RetrieveAPIMSubscriptionRepoQuery request, CancellationToken cancellationToken)
        {
            var apimConfig = APIMSubscriptionConfigHelper.GetAPIMConfiguration(request.Request.Channel, request.Request.Environment);
            string uri = APIMSubscriptionConfigHelper.GetSubscriptionURI(apimConfig, request.Request.SubscriptionId);

            SubscriptionResponse subscriptionResponse = new();

            var serviceResponse = await _mediator.Send(new RetrieveAPIMSubscriptionServiceQuery(uri, HttpMethod.Get), cancellationToken);

            if (serviceResponse.Key == HttpStatusCode.OK)
            {
                string subscriptionKeysUri = APIMSubscriptionConfigHelper.GetSubscriptionKeysURI(apimConfig, request.Request.SubscriptionId);
                var subscriptionKeysResponse = await _mediator.Send(new GetAPIMSubscriptionKeysServiceQuery(subscriptionKeysUri, HttpMethod.Post), cancellationToken);
                var subscriptionKeys = Json.Deserialize<SubscriptionKeys>(subscriptionKeysResponse.Value);

                var apimSubscriptionResponse = Json.Deserialize<APIMSubscriptionResponse>(serviceResponse.Value);

                subscriptionResponse.SubscriptionState = apimSubscriptionResponse.Properties.State.Is("active") ? "Enabled" : "Disabled";
                var listSubscriptionKeys = new List<SubscriptionKey>
                        {
                        new SubscriptionKey { Key = subscriptionKeys.PrimaryKey, ValidFrom = string.Empty, ValidTo = string.Empty }
                        };
                subscriptionResponse.SubscriptionKeys = listSubscriptionKeys;
                subscriptionResponse.Channel = request.Request.Channel;
                subscriptionResponse.Environment = request.Request.Environment;
                subscriptionResponse.Status = Status.OK;
            }
            else if (serviceResponse.Key == HttpStatusCode.NotFound)
            {
                subscriptionResponse.Error = new ErrorDetails { Code = Constants.ErrorCodes.VE002, Message = Constants.ErrorCodes.VE002Reason };
                subscriptionResponse.Status = Status.OK;
            }
            else
            {
                subscriptionResponse.Error = new ErrorDetails { Code = Constants.ErrorCodes.G039, Message = Constants.ErrorCodes.G039Reason };
                subscriptionResponse.Status = Status.Error;
            }

            return subscriptionResponse;
        }
    }
}
