using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class HouseholdEventsSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<HouseholdEventsSubscriber> _logger;

        public HouseholdEventsSubscriber(IMediator mediator, ILogger<HouseholdEventsSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("HouseholdDissolveEventSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.TargetedMembershipEventTopicName, Constants.Application.HouseholdDissolveEventSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("HouseholdDissolveEventSubscriber - Start");
            try
            {
                await _mediator.Send(Json.Deserialize<HouseHoldDissolveCommand>(eventRequestPayload));
            }
            finally
            {
                _logger.LogInformation("HouseholdDissolveEventSubscriber - End");
            }
        }
    }
}
