using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCArcDocumentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class NDCArcDocumentUpdateSubscriber
    {
        private readonly IArcDocumentUpdateCommand  _arcDocumentUpdateCommand;

        public NDCArcDocumentUpdateSubscriber(IArcDocumentUpdateCommand  arcDocumentUpdateCommand)
        {
            _arcDocumentUpdateCommand = arcDocumentUpdateCommand ?? throw new ArgumentNullException(nameof(arcDocumentUpdateCommand));
        }

        [Function("NDCArcDocumentUpdateEventSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Environment.NDCArcDocumentUpdateEventHandlerTopicName, Constants.Environment.NDCArcDocumentUpdateSubscriptionName, Connection = "OrderServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);
            VAA.EAPI.Common.AsyncContext.Current?.MapFromCloudEvent(cloudEvent);
            await _arcDocumentUpdateCommand.Handle(cloudEvent);
        }
    }
}
