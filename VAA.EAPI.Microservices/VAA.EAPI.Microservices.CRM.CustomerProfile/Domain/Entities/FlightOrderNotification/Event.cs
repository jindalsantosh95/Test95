using System;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification
{
    public class Event
    {
        public string EventType { get; set; }
        public string EventSource { get; set; }
        public DateTime EventRaised { get; set; }
    }
}