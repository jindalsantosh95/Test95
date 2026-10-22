using System;
using Microsoft.Azure.Functions.Worker;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class PartnerDataMessageSubscriber
    {
        private readonly IPartnerDataEventHandler _handler;

        public PartnerDataMessageSubscriber(IPartnerDataEventHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("PartnerDataMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ReferenceDataTopicName, Constants.Application.PartnerDataSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var message = Json.Deserialize<PartnerDataMessage>(eventRequestPayload);
            await _handler.HandleAsync(message);
        }
    }
}
