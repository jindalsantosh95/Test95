using Azure.Messaging.ServiceBus;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging
{
    public class ServiceBusTopicMessageEventHandler(IAzureServiceBusTopicClient serviceBusClient) : INotificationHandler<CloudEvent>
    {
        private readonly IAzureServiceBusTopicClient _serviceBusClient = serviceBusClient;

        public async Task Handle(CloudEvent cloudEvent, CancellationToken cancellationToken)
        {
            Common.AsyncContext.Current?.MapToCloudEvent(cloudEvent);

            await _serviceBusClient.PublishMessageAsync(new ServiceBusMessage(Json.Serialize(cloudEvent)) { Subject = cloudEvent.Type, CorrelationId = cloudEvent.CorrelationId.ToString(), MessageId = cloudEvent.Id }, cloudEvent.Source);
        }
    }
}
