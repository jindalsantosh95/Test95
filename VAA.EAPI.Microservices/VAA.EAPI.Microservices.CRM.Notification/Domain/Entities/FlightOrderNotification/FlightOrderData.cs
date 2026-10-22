namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification
{
    public class FlightOrderData
    {
        public Flight EventFlight { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Journey> Journeys { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public NotificationType NotificationType { get; set; }
    }
}