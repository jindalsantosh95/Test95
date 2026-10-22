using System;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightOrderNotification
{
    public class Flight
    {
        public static explicit operator Flight(FlightBookingQuery source)
        {
            return new Flight
            {
                Departure = source.Departure,
                Arrival = source.Arrival,
                OperatingCarrier = source.OperatingCarrier,
                ScheduledDepartureUtcDateTime = source.ScheduledDepartureUtcDateTime,
                ScheduledDepartureLocalDateTime = source.ScheduledDepartureLocalDateTime,
                ScheduledArrivalLocalDateTime = source.ScheduledArrivalLocalDateTime,
                NotificationType = source.NotificationType
            };
        }

        public string Departure { get; set; }
        public string Arrival { get; set; }
        public Carrier OperatingCarrier { get; set; }
        public DateTime ScheduledDepartureUtcDateTime { get; set; }
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public string NotificationType { get; set; } = "CheckInOpen";
    }
}
