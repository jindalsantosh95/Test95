using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands.AddressUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Triggers.ServiceBus
{
    public class AddressChangeEventSubscriber
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressChangeEventSubscriber> _logger;

        public AddressChangeEventSubscriber(IMediator mediator, ILogger<AddressChangeEventSubscriber> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        [Function("AddressChangeEventSubscriber")]
        public async Task Delete([ServiceBusTrigger(Constants.Application.ServiceBusTopicName, Constants.Application.AddressChangeEventSubscriber, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            await _mediator.Send(Json.Deserialize<AddressChangeEventCommand>(eventRequestPayload));
        }
    }
}
