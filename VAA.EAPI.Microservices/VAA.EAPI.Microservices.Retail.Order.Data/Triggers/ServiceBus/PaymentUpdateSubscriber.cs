using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.PaymentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class PaymentUpdateSubscriber
    {
        private readonly IMediator _mediator;

        public PaymentUpdateSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        [Function("PaymentUpdateSubscriber")]

        public async Task Run([ServiceBusTrigger("%PaymentUpdateSubscriberTopicName%", "%PaymentUpdateSubscriptionName%", Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {

            var cloudEvent = Json.Deserialize<PaymentUpdateCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
