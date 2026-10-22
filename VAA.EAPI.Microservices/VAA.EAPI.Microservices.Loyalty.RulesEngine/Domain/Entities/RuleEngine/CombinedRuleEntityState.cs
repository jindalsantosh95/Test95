using System.Collections.Generic;


namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class CombinedRuleEntityState
    {
        public string CorrelationId { get; set; }
        public List<Customer> Customers { get; set; }
    }
}
