using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Domain
{
    public sealed class CarbonRequest
    {
        public int? Passengers { get; set; }
        public IEnumerable<Flight> Flights { get; set; }

        public static explicit operator CarbonRequest(SustainMSRequest source)
        {
            if (source == null) return null;

            return new()
            {
                Flights = source.FlightDetails.Select(x => (Flight)x),
                Passengers = source.Passengers
            };
        }
    }
}
