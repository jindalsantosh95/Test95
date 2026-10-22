using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.AddressHistoryCommand;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.ServiceBus
{
    public class AddressHistorySubscriber
    {
        private readonly IMediator _mediator;

        public AddressHistorySubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("CreateAddressHistoryEventSubscriber")]
        public async Task CreateAddressHistoryEventSubscriber([ServiceBusTrigger(Constants.Application.CustomerEventTopicName, Constants.Application.AddressHistoryCreateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<AddressHistoryCreateCommand>(eventRequestPayload));
        }
    }
}
