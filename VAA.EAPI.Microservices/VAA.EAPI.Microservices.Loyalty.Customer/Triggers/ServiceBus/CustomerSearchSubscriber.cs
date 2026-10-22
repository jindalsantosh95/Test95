using MediatR;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerSearchCommands;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Triggers.ServiceBus
{
    public class CustomerSearchSubscriber
    {
        private readonly IMediator _mediator;

        public CustomerSearchSubscriber(IMediator mediator)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
        }

        [Function("UpdateCustomerSearchEventSubscriber")]
        public async Task UpdateCustomerSearchEventSubscriber([ServiceBusTrigger(Constants.Application.CustomerEventTopicName, Constants.Application.CustomerSearchUpdateEventSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<CustomerSearchUpdateCommand>(eventRequestPayload));
        }

        [Function("EnrichAuditDataWithCustDetailsEventSubscriber")]
        public async Task EnrichAuditDataWithCustDetailsEventSubscriber([ServiceBusTrigger(Constants.Application.CustomerEventTopicName, Constants.Application.EnrichAuditDataWithCustDetailsEventSubscriberName, Connection = "ServiceBusConnectionString")] string eventRequestPayload)
        {
            await _mediator.Send(Json.Deserialize<CustomerSearchAuditCommand>(eventRequestPayload));
        }
    }
}