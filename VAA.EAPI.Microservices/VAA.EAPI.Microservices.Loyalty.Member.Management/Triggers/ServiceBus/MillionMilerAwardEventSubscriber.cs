using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class MillionMilerAwardEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<MillionMilerAwardEventSubscriber> _logger;

        public MillionMilerAwardEventSubscriber(IMediator mediator, ILogger<MillionMilerAwardEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("MillionMilerAwardEventSubscriber")]
        public async Task Update([ServiceBusTrigger(Constants.Application.ServiceBusTopicName, Constants.Application.MillionMilerAwardEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            _logger.LogInformation("MillionMilerAwardEventSubscriber");
            await _mediator.Send(Json.Deserialize<MillionMilerEmailCommand>(eventRequestPayload));
        }
    }
}
