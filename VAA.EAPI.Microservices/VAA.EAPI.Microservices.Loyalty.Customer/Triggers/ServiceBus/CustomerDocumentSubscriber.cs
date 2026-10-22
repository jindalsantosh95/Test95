using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.ServiceBus
{
    public class CustomerDocumentSubscriber
    {
        private readonly IMediator _mediator;

        public CustomerDocumentSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("UpdateCustomerDocEventSubscriber")]
        public async Task UpdateCustomerDocEventSubscriber([ServiceBusTrigger(Constants.Application.CustomerEventTopicName, Constants.Application.CustomerDocUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<CustomerUpdateCommand>(eventRequestPayload));
        }
    }
}