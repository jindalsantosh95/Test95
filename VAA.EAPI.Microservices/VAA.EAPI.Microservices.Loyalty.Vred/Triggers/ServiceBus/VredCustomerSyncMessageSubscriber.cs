using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain;


namespace VAA.EAPI.Microservices.Loyalty.Vred.Triggers.ServiceBus
{
    public class VredCustomerSyncMessageSubscriber
    {
        private readonly IMediator _mediator;

        public VredCustomerSyncMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("VredCustomerSyncSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.VredCustomerSyncEventHandlerTopicName, Constants.Application.VredCustomerSyncEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var log = context.GetLogger("VredCustomerSyncMessageSubscriber");
            log.LogInformation("VredCustomerSyncMessageSubscriber message received");
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(Json.Deserialize<CreateCustomerEntities>(Convert.ToString(cloudEvent.Data)));
        }
    }
}
