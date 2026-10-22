using Azure;
using Azure.Messaging.EventGrid;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Mapper;


namespace VAA.EAPI.Infrastructure.MessageBroker.EventGridPublisher
{
    public class EventHandler : INotificationHandler<EventGridTopicEvent>
    {
        public async Task Handle(EventGridTopicEvent eventGridTopicEvent, CancellationToken cancellationToken)
        {
            var eventGridTopicEventData = EventGridRequestMapping.OrderChangeRequestEventMapper(eventGridTopicEvent);
            EventGridPublisherClient client = new EventGridPublisherClient(new Uri(eventGridTopicEvent.Source), new AzureKeyCredential(eventGridTopicEvent.Key));
            await client.SendEventAsync(eventGridTopicEventData);
        }
    }
}
