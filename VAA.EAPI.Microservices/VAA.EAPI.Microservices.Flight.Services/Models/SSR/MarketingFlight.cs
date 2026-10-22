using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class MarketingFlight
    {
        public string OperatingSuffix { get; set; }
        public string FlightNumber { get; set; }
        public IEnumerable<Airline> MarketingAirlines { get; set; }

        public static explicit operator MarketingFlight(MarketingSegment source)
        {
            if (source == null)
                return null;

            return new MarketingFlight
            {
                OperatingSuffix = source.OperationalSuffix,
                FlightNumber = source.FlightNum,
                MarketingAirlines = source.Carrier?.Select(c => (Airline)c) ?? Enumerable.Empty<Airline>()
            };
        }
    }
}
