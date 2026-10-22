using Azure.Messaging.ServiceBus;
using System.Threading.Tasks;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging
{
    public interface IAzureServiceBusTopicClient
    {
        public Task PublishMessageAsync(ServiceBusMessage serviceBusMessage, string topicName);

    }
}
