using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator
{
    public class EarnAirlineAccrualCalculatorResponse
    {
        public IEnumerable<SegmentResponse> SegmentResponses { get; set; }
    }
}
