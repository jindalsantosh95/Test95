using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class SpecialServiceRequests
    {
        public BookingReference BookingReference { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}
