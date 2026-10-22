namespace VAA.EAPI.Microservices.CRM.Notification.Domain.Entities.FlightOrderNotification
{
    public class BookingReference
    {
        public string AirlineDesignator { get; set; }
        public string PnrLocator { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}