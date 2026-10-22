using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption
{
    public class InRuleEvaluationRequest
    {
        public Event Event { get; set; }

        public List<Order> Order { get; set; }
    }
}
