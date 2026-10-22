using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Tests
{
    public static class RuleEngineTestHelpers
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"TierRuleSetApplicationName","TierManagementRules" },
                {"TierRuleSetEntityName","tierAwardPayloadInbound" },
                {"CombinedRuleApplicationName", "CombinedMemberUpdateAccrualCampaignVoucherRules" },
                {"CombinedRuleRequestEntityName", "memberUpdateCampaignPayloadInbound" },
                {"RuleAppPassword", "XXXX" },
                {"RuleAppUserName", "test" },
                {"RuleAppRepositoryServiceUri","https://dummy.com/123/" },
                {"ServiceBusTopicName","ruleengineeventmessages" },
                { "CustomerTransactionEventTopicName", "customertransactioneventmessages"},
                { "CommonCachingSlidingExpiration", "2:0:0"},
                { "CommonCachingAbsoluteExpiration","2:0:0"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
