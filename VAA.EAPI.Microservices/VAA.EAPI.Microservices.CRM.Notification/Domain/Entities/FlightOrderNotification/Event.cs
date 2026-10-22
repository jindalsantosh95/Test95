namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification
{
    public class Event
    {
        public string EventType { get; set; }
        public string EventSource { get; set; }
        public DateTime EventRaised { get; set; }
    }
}