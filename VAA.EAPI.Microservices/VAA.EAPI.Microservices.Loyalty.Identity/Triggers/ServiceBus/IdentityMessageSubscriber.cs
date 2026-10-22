using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;
using VAA.EAPI.Microservices.Loyalty.Identity.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Triggers.ServiceBus
{
    public class IdentityMessageSubscriber
    {
        private readonly IMediator _mediator;
        public IdentityMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("IdentityMessageSubscriber")]

        public async Task Run([ServiceBusTrigger(Constants.Application.IdentityEventHandlerTopicName, Constants.Application.IdentityEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("IdentityMessageSubscriber");
            log.LogInformation("Identity Event received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            var identityEventRequest = IdentitySubToCommandMapper.IdentityEventRequestMapper(cloudEvent);
            await _mediator.Send(identityEventRequest);
        }

        [Function("AccountCreationSubscriber")]
        public async Task AccountCreationSubscriber([ServiceBusTrigger(Constants.Application.IdentityEventHandlerTopicName, Constants.Application.AccountCreationEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<AccountCreationCommand>(eventRequestPayload));
        }


    }
}
