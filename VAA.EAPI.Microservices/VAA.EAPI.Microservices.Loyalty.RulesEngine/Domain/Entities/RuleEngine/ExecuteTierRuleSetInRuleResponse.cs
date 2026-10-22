namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class ExecuteTierRuleSetInRuleResponse
    {
        public RuleActionsResponse ResponseEntity { get; set; }

        public bool HasRuntimeErrors { get; set; }
    }
}
