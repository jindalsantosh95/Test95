using VAA.EAPI.Domain.Events;

namespace VAA.EAPI.Microservices.CRM.Content.Models
{
    public class CloudEventTopic : CloudEvent
    {
        public string Topic { get; set; }
    }
}
