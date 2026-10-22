using Microsoft.Azure.Functions.Worker;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class NDCOrderHistoryUpdateSubscriber
    {
        private readonly INDCOrderHistoryUpdateCommand _orderHistoryUpdateCommand;

        public NDCOrderHistoryUpdateSubscriber(INDCOrderHistoryUpdateCommand orderHistoryUpdateCommand)
        {
            _orderHistoryUpdateCommand = orderHistoryUpdateCommand;
        }

        [Function("NDCOrderHistoryUpdateSubscriber")]

        public async Task Run([ServiceBusTrigger(Constants.Environment.NDCOrderHistoryUpdateSubscriberTopicName, Constants.Environment.NDCOrderHistoryUpdateSubscriptionName, Connection = "OrderServiceBusConnectionString")] string eventRequestPayload, CancellationToken cancellationToken)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _orderHistoryUpdateCommand.Handle(cloudEvent);
        }
    }
}
