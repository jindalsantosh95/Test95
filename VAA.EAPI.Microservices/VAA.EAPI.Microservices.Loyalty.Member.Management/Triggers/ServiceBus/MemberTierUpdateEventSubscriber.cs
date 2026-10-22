using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class MemberTierUpdateEventSubscriber
    {
        private readonly IMediator _mediator;

        public MemberTierUpdateEventSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("MemberTierUpdateEventSubscriber")]
        public async Task MemberTierUpdateEventAsyc([ServiceBusTrigger(Constants.Application.ServiceBusTopicName,
            Constants.Application.MemberTierUpdateEventSubscriptionName,
            Connection = "ServiceBusConnectionString")] string eventRequestPayload,
            FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<MemberTierUpdateEvent>(eventRequestPayload));
        }
    }
}