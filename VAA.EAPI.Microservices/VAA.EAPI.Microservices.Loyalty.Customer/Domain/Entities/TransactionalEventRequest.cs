using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities
{
    public class TransactionalEvent
    {
        public string Topic { get; set; }
        public string Id { get; set; }
        public string EventType { get; set; }
        public string Subject { get; set; }
        public DateTime EventTime { get; set; }
        public TransactionalEventData Data { get; set; }
        public string DataVersion { get; set; }
        public string MetadataVersion { get; set; }
    }
}
