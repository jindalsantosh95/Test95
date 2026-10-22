using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS
{
    public class AccrualPointsCalculatorRequest
    {
        public IEnumerable<FlightSegment> FlightSegments { get; set; }

        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }
    }
}
