using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Seat.Application.Command;

namespace VAA.EAPI.Microservices.Retail.Seat.Triggers.ServiceBus
{
    public class SeatDeletionMessageSubsriber
    {
        private readonly IMediator _mediator;

        public SeatDeletionMessageSubsriber(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Function("SeatDeletionMessageSubsriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.SeatDeletionServiceBusTopicName, Constants.Application.SeatDeletionEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            var seatDeletionEventProcessCommand = Json.Deserialize<OrderSeatDeletionEventProcessCommand>(eventRequestPayload);
            
            AsyncContext.Current.MapFromCloudEvent(seatDeletionEventProcessCommand);

            await _mediator.Send(seatDeletionEventProcessCommand);
        }
    }
}
