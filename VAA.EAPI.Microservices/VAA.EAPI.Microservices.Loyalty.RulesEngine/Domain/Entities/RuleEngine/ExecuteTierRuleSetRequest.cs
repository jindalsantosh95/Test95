using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine
{
    public class ExecuteTierRuleSetRequest
    {
        [Required]
        [JsonProperty("tierRuleSetEntityRequest")]
        public TierRuleSetEntityRequest TierRuleSetEntityRequest { get; set; }
    }
}
