using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Sustainability.Domain
{
    public sealed class SustainMSRequest
    {
        public int? Passengers { get; set; }
        public IEnumerable<FlightDetail> FlightDetails { get; set; }
    }
}
