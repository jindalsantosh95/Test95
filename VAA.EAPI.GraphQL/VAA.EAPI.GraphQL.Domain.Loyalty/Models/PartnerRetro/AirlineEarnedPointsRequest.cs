using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerRetro
{
    public class AirlineEarnedPointsRequest
    {
        public IEnumerable<FlightSegments> FlightSegments { get; set; }
        public IEnumerable<CustomerDetails> CustomerDetails { get; set; }
    }
}
