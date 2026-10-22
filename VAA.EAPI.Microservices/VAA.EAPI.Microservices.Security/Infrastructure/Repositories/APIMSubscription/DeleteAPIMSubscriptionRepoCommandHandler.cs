using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;
using VAA.EAPI.Microservices.Security.Infrastructure.Services.APIMSubscription;

namespace VAA.EAPI.Microservices.Security.Infrastructure.Repositories.APIMSubscription
{
    public class DeleteAPIMSubscriptionRepoCommandHandler : IRequestHandler<DeleteAPIMSubscriptionRepoCommand, SubscriptionResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DeleteAPIMSubscriptionRepoCommandHandler> _logger;

        public DeleteAPIMSubscriptionRepoCommandHandler(IMediator mediator, ILogger<DeleteAPIMSubscriptionRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        }
        public async Task<SubscriptionResponse> Handle(DeleteAPIMSubscriptionRepoCommand request, CancellationToken cancellationToken)
        {

            var serviceResponse = await _mediator.Send(new DeleteAPIMSubscriptionServiceCommand(request.Uri, HttpMethod.Delete), cancellationToken);
            var subscriptionResponse = new SubscriptionResponse();
            if (serviceResponse.Key == HttpStatusCode.OK)
            {
                subscriptionResponse.SubscriptionState = "Deleted";
                subscriptionResponse.SubscriptionKeys = new List<SubscriptionKey> { };
                subscriptionResponse.Channel = request.Request.Channel;
                subscriptionResponse.Environment = request.Request.Environment;
                subscriptionResponse.Status = Status.OK;
            }
            else if (serviceResponse.Key == HttpStatusCode.NoContent)
            {
                subscriptionResponse.Error = new ErrorDetails() { Code = Constants.ErrorCodes.VE002, Message = Constants.ErrorCodes.VE002Reason };
                subscriptionResponse.Status = Status.OK;
            }
            else
            {
                subscriptionResponse.Error = new ErrorDetails() { Code = Constants.ErrorCodes.G039, Message = Constants.ErrorCodes.G039Reason };
                subscriptionResponse.Status = Status.Error;
            }

            return subscriptionResponse;
        }
    }
}
