using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.Disruption
{
    public class RulesEvaluationResponse
    {
        public Event Event { get; set; }

        public InRuleResponse InRuleResponse { get; set; }

        public List<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
