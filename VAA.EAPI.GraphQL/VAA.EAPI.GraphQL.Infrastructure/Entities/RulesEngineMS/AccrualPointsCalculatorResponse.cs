using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS
{
    public class AccrualPointsCalculatorResponse
    {
        public IEnumerable<SegmentResponse> SegmentResponses { get; set; }
    }
}
