using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.NotificationMs
{
    public class FlightOrderData
    {
        public static explicit operator FlightOrderData(Domain.Entities.FlightOrderData source) => new()
        {
            EventFlight = (Flight)source.EventFlight,
            BookingReference = (BookingReference)source.BookingReference
        };

        public Flight EventFlight { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}
