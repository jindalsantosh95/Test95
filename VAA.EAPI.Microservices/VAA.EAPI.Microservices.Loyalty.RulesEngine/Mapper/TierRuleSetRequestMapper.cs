using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Mapper
{
    public class TierRuleSetRequestMapper
    {
        public static RuleEngineRequest TierRuleSetToInRuleMapper(TierRuleSetEntityRequest tierRuleSetRequest)
        {
            if (tierRuleSetRequest == null) throw new ArgumentNullException(nameof(tierRuleSetRequest));
            EntityState requestEntity = new EntityState
            {
                CorrelationId = Guid.NewGuid().ToString(),
                Customers = new List<TierRuleSetEntityRequest> { tierRuleSetRequest }
            };
            return new RuleEngineRequest
            {
                RuleApplicationName = ConfigReader.Get(Constants.EnvironmentSettings.TierRuleSetApplicationName),
                RuleEntityName = ConfigReader.Get(Constants.EnvironmentSettings.TierRuleSetEntityName),
                RequestEntity = requestEntity
            };
        }
    }
}