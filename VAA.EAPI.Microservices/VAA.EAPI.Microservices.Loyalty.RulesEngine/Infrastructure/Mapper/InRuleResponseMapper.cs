using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Mapper
{
    public class InRuleResponseMapper
    {
        public static InRuleResponse MapInRuleResponse(ApplyRuleRequest inRuleRequest, ApplyRuleResponse applyRuleResponse)
        {
            return new InRuleResponse
            {
                RuleApplicationName = inRuleRequest.RuleApp.RepositoryRuleAppRevisionSpec.RuleApplicationName,
                RuleEntityName = inRuleRequest.EntityName,
                ResponseEntity = Json.Deserialize<object>(applyRuleResponse.EntityState),
                ActiveNotifications = applyRuleResponse.ActiveNotifications,
                ActiveValidations = applyRuleResponse.ActiveValidations,
                HasRuntimeErrors = applyRuleResponse.HasRuntimeErrors,
                SessionId = applyRuleResponse.SessionId
            };
        }
    }
}
