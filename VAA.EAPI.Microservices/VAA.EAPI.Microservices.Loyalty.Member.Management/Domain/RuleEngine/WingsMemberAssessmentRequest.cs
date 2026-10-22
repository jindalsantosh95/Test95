using Newtonsoft.Json;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.RuleEngine
{
    public class WingsMemberAssessmentRequest
    {
        [JsonProperty("tierRuleSetEntityRequest")]
        public TierRuleSetEntityRequest TierRuleSetEntityRequest { get; set; }  
    }
}
