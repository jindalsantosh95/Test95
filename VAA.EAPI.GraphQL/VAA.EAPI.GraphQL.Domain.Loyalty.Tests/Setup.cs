using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                { Common.Constants.Environment.CampaignManagementMsUrl, "https://testurl.api.com" },
                { Common.Constants.Environment.AddressHistoryRetrievalUrl, "https://testurl.api.com" },
                { Common.Constants.Environment.ContactHistoryRetrievalUrl, "https://testurl.api.com" },
                { Common.Constants.Environment.CustomerDocByCustIdUrl, "https://testurl.api.com" },
                { Common.Constants.Environment.CustomerAppId, "xxx" },
                { Common.Constants.Environment.CustomerOcpKey, "xxx" },
                { Common.Constants.Environment.AuditAppId, "xxx" },
                { Common.Constants.Environment.AuditOcpKey, "xxx" },
                { Common.Constants.Environment.CustTransAppId, "xxx" },
                { Common.Constants.Environment.CustTransOcpKey, "xxx" },
                { Common.Constants.Environment.CustTransactionMsUrl,"http://tempuri.org" },

                { "CampaignManagement:AppId", "https://testurl.api.com" },
                { "CampaignManagement:OcpSubID", "https://testurl.api.com" },
                { "Enrolment:Url", "https://testurl.api.com" },
                { "Enrolment:OcpKey", "https://testurl.api.com" },
                { "Enrolment:AppId", "https://testurl.api.com" },
                { "MemberManagement:OcpSubID", "https://testurl.api.com" },
                { "MemberManagement:AppId", "https://testurl.api.com" },
                { "EnableCustomerMs", "false" },

                { Common.Constants.Environment.LoyaltySyncAppId, "xxx" },
                { Common.Constants.Environment.LoyaltySyncOcpSubId, "xxx" },
                { Common.Constants.Environment.LoyaltySync,"https://testurl.api.com" },

                { Common.Constants.Environment.InRuleAppId, "xxx" },
                { Common.Constants.Environment.InRuleOcpSubID, "xxx" },
                { Common.Constants.Environment.InRuleUrl,"https://testurl.api.com" },

                { Common.Constants.Environment.SkyMsAppId, "xxx" },
                { Common.Constants.Environment.SkyMSOcpKey, "xxx" },
                { Common.Constants.Environment.SkyMsGenericUrl,"https://testurl.api.com" },

                { "VRed:MsUrl","https://testurl.api.com" },
                { "RefDataMS:GenericUrl","https://testurl.api.com" },
                { "refdataapi:appid","https://testurl.api.com" },
                { "refdatams:appid","https://testurl.api.com" },
                { "refdatams:ocpsubid","https://testurl.api.com" },
                { "refdataapi:ocpsubid" ,"https://testurl.api.com" },
                { "vred:appid", "https://testurl.api.com" },
                { "vred:ocpsubid", "https://testurl.api.com" },
                { "ListOfStatusForAccrual", "Active,Frozen,Active Pending,ActivePending,Expired,Inactive" },
                { "CustTransactionMSURL","https://testurl.api.com" },
                {"CommonCachingSlidingExpiration", "12:0:0" },
                {"CommonCachingAbsoluteExpiration", "12:0:0" },
                {Common.Constants.Environment.AuditMsUrl, "https://domain/environment/LoyaltyAuditManagement" },
                {Common.Constants.Environment.LoyaltyMemberManagementMsUrl, "https://domain/environment/loyaltymembermanagement" },
                {Common.Constants.Environment.CustomerMsUrl, "https://domain/environment/microservices" },
                {"OrderSearch:MSUrl","https://testurl.api.com" },
                {"OrderSearch:AppId", "xxx"},
                {"OrderSearch:OcpSubId", "xxx"},
                {"OrderData:MSUrl","https://testurl.api.com" },
                {"OrderData:OcpSubID", "xxx"},
                {"OrderData:AppId", "xxx"},
                {"OrderManagement:MsUrl","https://testurl.api.com" },
                {"OrderManagement:AppId", "xxx"},
                {"OrderManagement:OcpKey", "xxx"},             
                {"AddressSearchbyPostcode:MsUrl","https://testurl.api.com"},             
                {"AddressLookup:AppId","testvalue" }
        };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
