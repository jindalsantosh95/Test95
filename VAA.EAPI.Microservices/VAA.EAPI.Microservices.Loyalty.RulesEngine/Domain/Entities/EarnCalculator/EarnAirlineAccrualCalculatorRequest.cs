using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator
{
    public class EarnAirlineAccrualCalculatorRequest
    {
        public IEnumerable<FlightSegment> FlightSegments { get; set; }

        public IEnumerable<CustomerDetail> CustomerDetails { get; set; }
    }
}