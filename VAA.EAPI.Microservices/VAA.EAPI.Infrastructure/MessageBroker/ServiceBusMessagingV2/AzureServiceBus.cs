using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2
{
    internal class AzureServiceBus : IMessageBus
    {
        private readonly ServiceBusSender _serviceBusSender;

        internal AzureServiceBus(ServiceBusSender serviceBusSender)
        {
            this._serviceBusSender = serviceBusSender;
        }

        public async Task PublishMessageAsync(CloudEvent cloudEvent)
        {
            Common.AsyncContext.Current?.MapToCloudEvent(cloudEvent);

            await _serviceBusSender.SendMessageAsync(
                new ServiceBusMessage(Json.Serialize(cloudEvent, false))
                {
                    Subject = cloudEvent.Type,
                    CorrelationId = cloudEvent.CorrelationId.ToString(),
                    MessageId = cloudEvent.Id
                }.AddEapiTraceContext());
        }

        internal static IMessageBus Create(ServiceBusSender sender)
        {
            return new AzureServiceBus(sender);
        }
    }
}
