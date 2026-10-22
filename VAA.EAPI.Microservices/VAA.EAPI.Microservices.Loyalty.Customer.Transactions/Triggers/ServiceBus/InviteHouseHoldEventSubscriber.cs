using System.Threading.Tasks;

using MediatR;

using Microsoft.Azure.Functions.Worker;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.InviteHouseHold;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class InviteHouseHoldEventSubscriber
    {
        private readonly IMediator _mediator;

        public InviteHouseHoldEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("InviteHouseHoldEventSubscriber")]
        public async Task Create([ServiceBusTrigger(Constants.Application.FlyingClubMemberEventTopicName, Constants.Application.CreatePendingHouseHoldMessageSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<InviteHouseHoldCreateEvent>(eventRequestPayload));
        }

        [Function("DeleteInviteHouseHoldDocEventSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.FlyingClubMemberEventTopicName, Constants.Application.DeletePendingHouseHoldMessageSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<InviteHouseHoldDeleteEvent>(eventRequestPayload));
        }
    }
}
