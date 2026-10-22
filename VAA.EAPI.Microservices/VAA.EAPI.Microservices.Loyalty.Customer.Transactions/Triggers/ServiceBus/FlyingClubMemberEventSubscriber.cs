using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class FlyingClubMemberEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FlyingClubMemberEventSubscriber> _logger;

        public FlyingClubMemberEventSubscriber(IMediator mediator, ILogger<FlyingClubMemberEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("UpdateFlyingClubMemberEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.FlyingClubMemberEventTopicName, Constants.Application.FlyingClubMemberUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            _logger.LogInformation("UpdateFlyingClubMemberEventSubscriber");
            await _mediator.Send(Json.Deserialize<FlyingClubMemberPatchEvent>(eventRequestPayload));
        }
    }
}
