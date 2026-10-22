using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.RefundUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class RefundUpdateSubscriber
    {
        private readonly IMediator _mediator;

        public RefundUpdateSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("VoidOrderUpdateSubscriber")]

        public async Task Run([ServiceBusTrigger("%RefundUpdateSubscriberTopicName%", "%RefundUpdateSubscriptionName%", Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {

            var cloudEvent = Json.Deserialize<RefundUpdateCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
