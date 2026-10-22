using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCNameCorrectionUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Triggers.ServiceBus
{
    public class NDCNameUpdateSubscriber
    {
        private readonly INameUpdateCommand _nameUpdateCommand;

        public NDCNameUpdateSubscriber(INameUpdateCommand nameUpdateCommand)
        {
            _nameUpdateCommand = nameUpdateCommand ?? throw new ArgumentNullException(nameof(_nameUpdateCommand));
        }

        [Function("NDCNameUpdateEventSubscriber")]
        public async Task Run([ServiceBusTrigger(Constants.Environment.NDCNameUpdateEventHandlerTopicName, Constants.Environment.NDCNameUpdateSubscriptionName, Connection = "OrderServiceBusConnectionString")] string eventRequestPayload, FunctionContext context)
        {
            var cloudEvent = Json.Deserialize<CloudEvent>(eventRequestPayload);

            AsyncContext.Current.MapFromCloudEvent(cloudEvent);

            await _nameUpdateCommand.Handle(cloudEvent);
        }
    }
}
