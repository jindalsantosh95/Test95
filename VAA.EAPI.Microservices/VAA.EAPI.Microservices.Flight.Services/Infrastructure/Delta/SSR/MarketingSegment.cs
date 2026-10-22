using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class MarketingSegment
    {
        public string OperationalSuffix { get; set; }
        public string FlightNum { get; set; }
        public IEnumerable<Carrier> Carrier { get; set; }
    }
}
