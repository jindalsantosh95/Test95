using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Helper;
using VAA.EAPI.Microservices.Security.Infrastructure.Mapper.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Models.APIMSubscriptionResponse;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class CreateAPIMSubscriptionRepoCommandHandler : IRequestHandler<CreateAPIMSubscriptionRepoCommand, SubscriptionResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateAPIMSubscriptionRepoCommandHandler> _logger;

        public CreateAPIMSubscriptionRepoCommandHandler(IMediator mediator, ILogger<CreateAPIMSubscriptionRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<SubscriptionResponse> Handle(CreateAPIMSubscriptionRepoCommand request, CancellationToken cancellationToken)
        {
            var apimConfig = APIMSubscriptionConfigHelper.GetAPIMConfiguration(request.Request.Channel, request.Request.Environment);

            var apimRequest = APIMSubscriptionRequestMapper.MapToMSAPIMSubscriptionCreateRequest(apimConfig, request.Request.UserAccount.UserId, request.Request.SubscriptionName);
            string uri = APIMSubscriptionConfigHelper.GetSubscriptionURI(apimConfig, request.Request.SubscriptionId);
            var subscriptionResponse = new SubscriptionResponse();
            APIMSubscriptionResponse apimSubscriptionResponse;

            var serviceResponse = await _mediator.Send(new CreateAPIMSubscriptionServiceCommand(apimRequest, uri), cancellationToken);

            if (serviceResponse.Key == HttpStatusCode.Created)
            {
                if (serviceResponse.Value != null)
                {
                    apimSubscriptionResponse = Json.Deserialize<APIMSubscriptionResponse>(serviceResponse.Value);

                    if (apimSubscriptionResponse == null)
                        _logger.LogError("Create APIMSubscription RepoCommand Handler || No data mapped to Create APIMSubscription RepoCommand Handler");
                    else
                    {
                        subscriptionResponse.SubscriptionState = "Enabled";
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
            else if (serviceResponse.Key == HttpStatusCode.OK)
            {
                subscriptionResponse.Error = new ErrorDetails { Code = Constants.ErrorCodes.VE001, Message = Constants.ErrorCodes.VE001Reason };
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
