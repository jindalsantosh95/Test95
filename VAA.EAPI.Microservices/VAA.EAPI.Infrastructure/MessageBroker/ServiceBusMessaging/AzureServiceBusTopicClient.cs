using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging
{
    public class AzureServiceBusTopicClient(ServiceBusClient serviceBusClient) : IAzureServiceBusTopicClient
    {
        private ServiceBusClient _serviceBusClient { get; set; } = serviceBusClient;

        public async Task PublishMessageAsync(ServiceBusMessage serviceBusMessage, string topicName)
        {
            ServiceBusSender serviceBusSender = _serviceBusClient.CreateSender(topicName);

            try
            {
                await serviceBusSender.SendMessageAsync(serviceBusMessage.AddEapiTraceContext());
            }
            finally
            {
                await serviceBusSender.CloseAsync();
            }
        }
    }
}
