using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class WingsMemberAssessmentActionsResponse
    {
        public string CustomerId { get; set; }

        public WingsMemberAssessmentRuleResponse RuleResponse { get; set; }             
    }
}
