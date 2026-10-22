using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.EventHandlers.Reference;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Reference;


namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Triggers.ServiceBus
{
    public class ProductDataMessageSubscriber
    {

        private readonly IProductDataEventHandler _handler;

        public ProductDataMessageSubscriber(IProductDataEventHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("ProductDataMessageSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Application.ReferenceDataTopicName, Constants.Application.ProductDataSubscriptionName, Connection = "ServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var message = Json.Deserialize<ProductDataMessage>(eventRequestPayload);
            await _handler.HandleAsync(message);
        }
    }
}

