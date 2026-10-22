using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.EventHandlers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class LTGEventsSubscriber
    {
        private readonly IMediator _mediator;

        public LTGEventsSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("LTGAwardEventSubscriber")]
        public async Task LTGAwardEventSubscriber([ServiceBusTrigger(Constants.Application.TargetedMembershipEventTopicName, Constants.Application.LTGAwardEventSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<LTGAwardCloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _mediator.Send(cloudEvent);
        }
    }
}
