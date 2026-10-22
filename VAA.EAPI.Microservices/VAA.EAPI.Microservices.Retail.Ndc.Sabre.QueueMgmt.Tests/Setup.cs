using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.QueueMgmt.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                { EAPI.Infrastructure.Constants.Constants.KeyVaultUri, "https://tempuri.org" },
                { EAPI.Infrastructure.NDC.Sabre.Constants.Environment.Domain, "Domain" },
                { Constants.Environment.ConversationId, "ConversationId" },
                { Constants.Environment.FromPartyId, "FromPartyId" },
                { Constants.Environment.ToPartyId, "ToPartyId" },
            };

            foreach (var item in settings)
            {
                ConfigReader.Set(item.Key, item.Value);
            }
        }
    }
}

