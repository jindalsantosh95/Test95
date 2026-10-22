using Azure.Messaging.EventGrid;
using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Infrastructure.Mapper
{
    public class EventGridRequestMapping
    {
        public static EventGridEvent OrderChangeRequestEventMapper(EventGridTopicEvent eventGridTopicEventData)
        {
            return new EventGridEvent(eventGridTopicEventData.Subject, eventGridTopicEventData.Type, eventGridTopicEventData.Specversion, eventGridTopicEventData.Data)
            {
                Id = eventGridTopicEventData.Id
            };
        }
    }
}
