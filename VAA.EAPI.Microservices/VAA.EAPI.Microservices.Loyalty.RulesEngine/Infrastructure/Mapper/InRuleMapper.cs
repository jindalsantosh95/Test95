using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.RuleEngine;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Infrastructure.Mapper
{
    public class InRuleMapper
    {
        public static ApplyRuleRequest MapInRuleRequest(RuleEngineRequest req)
        {
            ApplyRuleRequest applyRuleRequest = new ApplyRuleRequest
            {
                RuleApp = new RuleApp
                {
                    Password = ConfigReader.Get(Constants.EnvironmentSettings.RuleAppPassword),
                    UserName = ConfigReader.Get(Constants.EnvironmentSettings.RuleAppUserName),
                    RepositoryServiceUri = ConfigReader.Get(Constants.EnvironmentSettings.RuleAppRepositoryServiceUri),
                    RepositoryRuleAppRevisionSpec = new RepositoryRuleAppRevisionSpec
                    {
                        RuleApplicationName = req.RuleApplicationName
                    },
                },
                EntityName = req.RuleEntityName,
                EntityState = Json.Serialize(req.RequestEntity)
            };
            return applyRuleRequest;

        }
    }
}
