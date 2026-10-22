using System;
using VAA.EAPI.Domain.Entities.Email;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain
{
    public class EmailTriggeringEventRequest
    {
        public string Topic { get; set; }
        public string Id { get; set; }
        public string EventType { get; set; }
        public string Subject { get; set; }
        public DateTime? EventTime { get; set; }
        public SendEmailRequest Data { get; set; }
        public string DataVersion { get; set; }
        public string MetadataVersion { get; set; }
    }
}
