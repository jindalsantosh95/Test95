using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;

namespace VAA.EAPI.Microservices.Retail.Seat.Triggers.ServiceBus
{
    public class SeatFulfilmentMessageSubscriber
    {
        private readonly IMediator _mediator;

        public SeatFulfilmentMessageSubscriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("SeatFulfilmentMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.SeatFulFillmentTopicName, Constants.Application.SeatFulfilmentEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var seatFulfilmentEventProcessCommand = Json.Deserialize<SeatFulfilmentEventProcessCommand>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(seatFulfilmentEventProcessCommand);

            await _mediator.Send(seatFulfilmentEventProcessCommand);
        }
    }
}
