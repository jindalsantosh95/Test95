using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.CRM.Tests
{
    public class CRMTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"CRMWebApi:PatchEndpointAPIM", "https://Testresources.virginatlantic.com/crm/{crm_entity}({entityId})"},
                {"CRMWebApi:OcpSubKey","https://avadeunkveapis.vault.azure.net/secrets/AVADEUNAFNCRMMICROSERV-Config-CRMWebApi-OcpSubKey/)"},
                {"CRMWebApi:GetEndpointAPIM","https://Testresources.virginatlantic.com/crm/{crm_entity}"},
                {"ResponsePollingCount","1"},
                {"ResponseDelayTimeInMS","100"},
                {"CRMWebApi:PaymentUpdateResponseUrl","https://Testresources.virginatlantic.com/dev/crmmicroservice/updatepaymentstatus/{correlationid}"},
                {"CRMWebApi:ResponsiveFeature","true"},
                {"CRMPaymentEventHandlerTopicName", "paymentupdaterequesteventmessage" },
                {"AzureAd:ClientId", "testdata" },
                {"AzureAd:ClientSecret", "testdata" },
                {"AzureAd:Instance", "https://tempuri.org" },
                {"AzureAd:TenantId", "testdata" },
                {"crmwebapi:endpoint", "testdata" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
