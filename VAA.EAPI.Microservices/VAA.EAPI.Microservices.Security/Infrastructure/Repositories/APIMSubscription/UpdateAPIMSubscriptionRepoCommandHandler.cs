using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Infrastructure.Mapper.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;


namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class UpdateAPIMSubscriptionRepoCommandHandler : IRequestHandler<UpdateAPIMSubscriptionRepoCommand, SubscriptionResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateAPIMSubscriptionRepoCommandHandler> _logger;

        public UpdateAPIMSubscriptionRepoCommandHandler(IMediator mediator, ILogger<UpdateAPIMSubscriptionRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<SubscriptionResponse> Handle(UpdateAPIMSubscriptionRepoCommand request, CancellationToken cancellationToken)
        {
            var apimConfig = APIMSubscriptionConfigHelper.GetAPIMConfiguration(request.Request.Channel, request.Request.Environment);

            var apimRequest = APIMSubscriptionRequestMapper.MapToMSAPIMSubscriptionUpdateRequest(apimConfig, request.Request.Enabled);
            string uri = APIMSubscriptionConfigHelper.GetSubscriptionURI(apimConfig, request.Request.SubscriptionId);
            var subscriptionResponse = new SubscriptionResponse();
            APIMSubscriptionResponse apimSubscriptionResponse;
            var serviceResponse = await _mediator.Send(new UpdateAPIMSubscriptionServiceCommand(apimRequest, uri), cancellationToken);

            if (serviceResponse.Key == HttpStatusCode.OK)
            {
                if (serviceResponse.Value != null)
                {

                    apimSubscriptionResponse = Json.Deserialize<APIMSubscriptionResponse>(serviceResponse.Value);

                    if (apimSubscriptionResponse == null)
                    {
                        _logger.LogError("Update APIM Subscription Repo CommandHandler || No data mapped to UpdateAPIMSubscriptionRepoCommandHandler");
                    }
                    else
                    {
                        subscriptionResponse.SubscriptionState = apimSubscriptionResponse.Properties.State.Is("active") ? "Enabled" : "Disabled";
                        var subscriptionKeys = new List<SubscriptionKey>
                        {
                          new SubscriptionKey { Key = apimSubscriptionResponse.Properties.PrimaryKey, ValidFrom = string.Empty, ValidTo = string.Empty },
                          //new SubscriptionKey { Key = apimSubscriptionResponse.Properties.SecondaryKey, ValidFrom = string.Empty, ValidTo = string.Empty }
                        };
                        subscriptionResponse.SubscriptionKeys = subscriptionKeys;
                        subscriptionResponse.Channel = request.Request.Channel;
                        subscriptionResponse.Environment = request.Request.Environment;
                        subscriptionResponse.Status = Status.OK;
                    }
                }
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
