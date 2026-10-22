namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class WingsMemberAssessmentInRuleResponse
    {
        public WingsMemberAssessmentActionsResponse ResponseEntity { get; set; }

        public bool HasRuntimeErrors { get; set; }
    }
}
