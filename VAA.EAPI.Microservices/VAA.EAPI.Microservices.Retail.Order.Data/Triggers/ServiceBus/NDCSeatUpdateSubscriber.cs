using MediatR;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCSeatUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class NDCSeatUpdateSubscriber
    {
        private readonly IMediator _mediator;
        
        public NDCSeatUpdateSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));            
        }

        [Function("NDCSeatUpdateSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Environment.NDCSeatUpdateSubscriberTopicName, Constants.Environment.NDCSeatUpdateSubscriptionName, Connection = "OrderServiceBusConnectionString")] string eventRequestPayload)
        {
            var cloudEvent = Json.Deserialize<SeatUpdateCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _mediator.Send(cloudEvent);
        }
    }
}
