using MediatR;
using System;

namespace VAA.EAPI.Domain.Events
{
    [Obsolete("Use VAA.EAPI.Domain.Events.CloudEvent and Azure Service Bus")]
    public class EventGridTopicEvent : INotification
    {
        public string Specversion { get; set; }
        public string Type { get; set; }
        public string Source { get; set; }
        public string Id { get; set; }
        public string Time { get; set; }
        public string Subject { get; set; }
        public string Dataschema { get; set; }
        public object Data { get; set; }
        public string Key { get; set; }

    }
}
