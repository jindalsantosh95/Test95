using Azure.Messaging.ServiceBus;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessaging;
using VAA.EAPI.Infrastructure.MessageBroker.ServiceBusMessagingV2;
using VAA.EAPI.Microservices.CRM.Content.Models;

namespace VAA.EAPI.Microservices.CRM.Content.Application
{
    public class ServiceBusHelper : IMessageBus
    {
        private readonly ServiceBusClient _serviceBusClient;

        public ServiceBusHelper(ServiceBusClient serviceBusClient)
        {
            _serviceBusClient = serviceBusClient ?? throw new ArgumentNullException(nameof(serviceBusClient));
        }

        public async Task PublishMessageAsync(CloudEvent cloudEvent)
        {
            var topic = ((CloudEventTopic)cloudEvent).Topic;
            await using var serviceBusSender = _serviceBusClient.CreateSender(topic);

            var sbmessage = new ServiceBusMessage(Json.Serialize(cloudEvent, false))
            {
                Subject = cloudEvent.Subject,
                CorrelationId = cloudEvent.Id,
                MessageId = cloudEvent.Id
            };

            await serviceBusSender.SendMessageAsync(sbmessage.AddEapiTraceContext());
        }
    }
}
