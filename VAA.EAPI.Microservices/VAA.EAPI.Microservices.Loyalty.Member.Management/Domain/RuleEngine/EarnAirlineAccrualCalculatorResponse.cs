using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class EarnAirlineAccrualCalculatorResponse
    {
        public IEnumerable<SegmentResponse> SegmentResponses { get; set; }
    }
}
