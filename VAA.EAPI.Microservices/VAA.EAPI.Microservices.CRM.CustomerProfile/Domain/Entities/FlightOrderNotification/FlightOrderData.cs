using System.Collections.Generic;

namespace VAA.EAPI.Microservices.CRM.CustomerProfile.Domain.Entities.FlightOrderNotification
{
    public class FlightOrderData
    {
        public Event Event { get; set; }
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Journey> Journeys { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}