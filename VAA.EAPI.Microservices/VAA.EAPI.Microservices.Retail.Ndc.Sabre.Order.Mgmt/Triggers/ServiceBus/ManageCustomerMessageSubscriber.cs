using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Events;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Triggers.ServiceBus
{
    public class ManageCustomerMessageSubscriber
    {
        private readonly IMediator _mediator;

        public ManageCustomerMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("ManageCustomerMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ManageCustomerEventHandlerTopicName, Constants.Application.ManageCustomerEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<ManageCustomerEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
