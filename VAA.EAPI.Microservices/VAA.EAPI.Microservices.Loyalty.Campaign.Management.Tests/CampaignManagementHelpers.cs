using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Tests
{
    public static class CampaignManagementHelpers
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                { "CommandStatusLocation", "http://localhost:7071/api/{0}s/status/{1}"},
                {"ServiceBusTopicName", "campaignmanagementeventmessages"},
                {"CustomerTranscationTopicName", "customertransactioneventmessages"},
                {"LoyaltyMemberServiceBusTopicName", "loyaltymembermanagementeventmessages"},
                {"EmailTriggerTopicName", "enrolcustomer"},
                {"CustomerTransactionMicroServiceDBKey", "XXXX" },
                {"CustomerTransactionMicroServiceEventStoreTableName", "CustomerTransactionEvents" },
                {"CustomerTransactionMicroServiceResponseTableName", "CustomerTransactionEventResponses" },
                {"CustomerServiceBusTopicName", "customertransactioneventmessages"},
                {"CampaignPromotionDelinkResponseUrl", "http://localhost:7071/api/{0}s/status/{1}" },
                {"PromotionCreateResponseUrl","http://localhost:7071/api/{0}s/status/{1}" },
                {"PromotionUpdateResponseUrl","http://localhost:7071/api/{0}s/status/{1}" },
                {"Customer:UrlGetbyCustID","https://testurl.api.com/customer/{customerid}" },
                {"Customer:UrlGetByMemNum","https://testurl.api.com/customer/loyalty/{memberid}" },
                {"Customer:ResourceId", "XXXXX"},
                {"CustomerTxnMSDetailsEndpoint", "https://testurl.api.com/customertransactions/"},
                {"CustomerTxnMS:ResourceId", "YYYYYY"},
                {"CommonCachingSlidingExpiration", "12:0:0" },
                {"CommonCachingAbsoluteExpiration", "12:0:0" },
                {"CampaignManagement:AppId", "abc123" },
                {"CampaignManagement:ClientSecret", "XcVb" },
                {"CampaignManagement:OcpSubID", "12312" },
                {"AzureADEndpoint", "https://dummy.com/123/" },
                {"ContainerNameForCampaigns", "test" },
                {"ContainerNameForPromotions", "test" },
                {"BlobConnStringForDownloads", "https://dummy.com/123/"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
