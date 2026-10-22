using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class AddressUpdateNotificationEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressUpdateNotificationEventSubscriber> _logger;

        public AddressUpdateNotificationEventSubscriber(IMediator mediator, ILogger<AddressUpdateNotificationEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("AddressUpdateNotificationEventSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.ServiceBusTopicName, Constants.Application.AddressUpdateNotificationEventSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<AddressUpdateNotificationCommand>(eventRequestPayload));
        }
    }
}
