using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper
{
    public class CombinedRuleSetRequestMapper
    {
        public static RuleEngineRequest CombinedRuleSetToInRuleMapper(CombinedRuleSetEntityRequest combinedRuleSetRequest)
        {
            if (combinedRuleSetRequest == null) throw new ArgumentNullException(nameof(combinedRuleSetRequest));
            CombinedRuleRequestEntityState requestEntity = new CombinedRuleRequestEntityState
            {
                CorrelationId = Guid.NewGuid().ToString(),
                Customers = new List<CombinedRuleSetEntityRequest> { combinedRuleSetRequest }
            };
            return new RuleEngineRequest
            {
                RuleApplicationName = ConfigReader.Get(Constants.EnvironmentSettings.CombinedRuleApplicationName),
                RuleEntityName = ConfigReader.Get(Constants.EnvironmentSettings.CombinedRuleRequestEntityName),
                RequestEntity = requestEntity
            };
        }
    }
}
