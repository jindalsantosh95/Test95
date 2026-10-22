using System;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class FlightBookingQuery
    {
        public string Departure { get; set; }
        public string Arrival { get; set; }
        public Carrier OperatingCarrier { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public string NotificationType { get; set; } = "CheckInOpen";
    }
}
