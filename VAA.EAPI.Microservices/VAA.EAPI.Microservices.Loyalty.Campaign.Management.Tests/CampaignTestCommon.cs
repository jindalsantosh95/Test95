using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public static class CampaignTestCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
