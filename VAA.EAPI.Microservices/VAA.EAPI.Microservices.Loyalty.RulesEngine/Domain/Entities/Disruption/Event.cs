
using System;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption
{
    public class Event
    {
        public string EventId { get; set; }

        public string EventType { get; set; }

        public string EventVersion { get; set; }

        public string Source { get; set; }

        public DateTime? OccurredAt { get; set; }

        public string CorrelationId { get; set; }
    }
}
