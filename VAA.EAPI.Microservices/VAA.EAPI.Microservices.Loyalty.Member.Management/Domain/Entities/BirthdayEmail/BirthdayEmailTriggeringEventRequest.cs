using System;
using VAA.EAPI.Domain.Entities.Email;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayEmail
{
    public class BirthdayEmailTriggeringEventRequest
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
