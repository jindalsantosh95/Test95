using System.Collections.Generic;

namespace VAA.EAPI.Orchestration.Notifications.Domain.Entities
{
    public class FlightOrderData
    {
        public Flight EventFlight { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Journey> Journeys { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public IEnumerable<SeatAllocation> Seats { get; set; }
        public bool Irop { get; set; }

    }
}
