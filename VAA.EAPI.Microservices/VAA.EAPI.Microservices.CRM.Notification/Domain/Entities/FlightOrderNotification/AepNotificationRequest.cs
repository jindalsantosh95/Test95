namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification
{
    public class AepNotificationRequest
    {
        public Passenger passenger {  get; set; }
        public Event Event { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Journey> Journeys { get; set; }
    }
}
