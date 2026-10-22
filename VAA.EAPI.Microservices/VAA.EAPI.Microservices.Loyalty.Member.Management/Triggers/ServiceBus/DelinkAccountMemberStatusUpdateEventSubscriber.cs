using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class DelinkAccountMemberStatusUpdateEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DelinkAccountMemberStatusUpdateEventSubscriber> _logger;

        public DelinkAccountMemberStatusUpdateEventSubscriber(IMediator mediator, ILogger<DelinkAccountMemberStatusUpdateEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("DelinkAccountMemberStatusUpdateEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.DelinkAccountMemberStatusUpdateEventTopicName, Constants.Application.MemberStatusEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("DelinkAccountMemberStatusUpdateEventSubscriber");
            await _mediator.Send(Json.Deserialize<DelinkAccountOnMemberStatusUpdateCommand>(eventRequestPayload));
        }
    }
}
