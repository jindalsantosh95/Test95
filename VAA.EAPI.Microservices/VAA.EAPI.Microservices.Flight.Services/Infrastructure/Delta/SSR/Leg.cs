using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class Leg
    {
        public int LegId { get; set; }
        public int Ordinal { get; set; }
        public IEnumerable<Carrier> Carrier { get; set; }
        public string FlightNum { get; set; }
        public TransportOriginDetails TransportOrigin { get; set; }
        public TransportDestinationDetails TransportDestination { get; set; }
    }
}
