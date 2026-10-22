using System;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper
{
    public class ActionEventDataMapper
    {
        public static RuleActionsResponse MapActionEventData(TierRuleSetEntityRequest tierRuleSetRequest, TierRuleEntityState resEntity)
        {
            if (resEntity == null) throw new ArgumentNullException(nameof(resEntity));
            return new RuleActionsResponse
            {
                CustomerId = tierRuleSetRequest.CustomerId,
                MemberNumber = tierRuleSetRequest.MemberNumber,
                RuleResponse = resEntity?.Customers[0]?.RuleResponse
            };
        }

        public static RuleActionsResponse MapActionEventData(CombinedRuleSetEntityRequest combinedRuleSetRequest, CombinedRuleEntityState resEntity)
        {
            if (resEntity == null) throw new ArgumentNullException(nameof(resEntity));
            return new RuleActionsResponse
            {
                CustomerId = combinedRuleSetRequest.CustomerId,
                MemberNumber = combinedRuleSetRequest.MemberNumber,
                ProductId = combinedRuleSetRequest?.ProductId,
                MemberStatus = combinedRuleSetRequest.MemberStatus,
                ProductName = combinedRuleSetRequest?.ProductName,
                RuleResponse = resEntity?.Customers[0]?.RuleResponse
            };
        }
    }

}
