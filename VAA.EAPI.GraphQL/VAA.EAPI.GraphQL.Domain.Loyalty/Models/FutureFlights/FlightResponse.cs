using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.FutureFlights
{
    public class FlightResponse
    {
        public IEnumerable<FlightSegmentDetail> FlightSegmentDetails { get; set; }
        public ErrorResponses ErrorResponses { get; set; }
    }
}
