using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class CombinedRuleRequestEntityState
    {
        public string CorrelationId { get; set; }
        public List<CombinedRuleSetEntityRequest> Customers { get; set; }
    }
}
