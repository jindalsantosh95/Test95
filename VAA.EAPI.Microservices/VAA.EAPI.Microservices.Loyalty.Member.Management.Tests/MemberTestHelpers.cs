using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Tests
{
    public static class MemberTestHelpers
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                {"LoyaltyMbrMgmt:AppId", "123" },
                {"LoyaltyMbrMgmt:ClientSecret", "DUMMY" },
                {"CustomerTxnMS:ResourceId", "dummy123" },
                {"LoyaltyMbrMgmt:OcpSubID", "123" },
                {"AzureADEndpoint", "https://dummy.com/123/" },
                { "CustomerTxnMSDetailsEndpoint", "http://localhost:7071/api/{0}/{1}"},
                { "CommandStatusLocation", "http://localhost:7071/api/{0}s/status/{1}"},
                { "CommandStatusPollingDelayMS", "30"},
                { "CommandStatusPollingRetryCount", "3"},
                {"EmailTriggerTopicName","enrolcustomer" },
                {"HoHLinkHeadSilverEmalTemplate","FCGOL-01" },
                {"YoungFlyerEnrolmentConfirmationEmailTemplate","vaaLoyaltyU18Parent" },
                {"HoHLinkChildSilverEmalTemplate","FCGOL-01A" },
                {"BirthdayEmailTemplate","FCGEN-19" },
                {"RuleEngineTopicName","ruleengineeventmessages" },
                {"GroupDocType", "Group" },
                {"CustomerTransactionTopicName", "customertransactioneventmessages" },
                {"BlobConnStringForEnrolledMemberToPromotion", "dummy123" },
                {"ContainerNameForEnrolledMemberToPromotion", "test" },
                {"CustomerMSUrl", "http://localhost:7071/api/{0}/{1}"},
                {"HoHAddressUpdateEmailTemplate", "FCGOL-02"},
                {"IsManualRun", "False"},
                {"BirthdayDateForManualRun", "2023-09-15"},
                {"ContainerNameForAddSSRResponse", "testResponse" },
                {"ContainerNameForAddSSRFile", "test" },
                {"ArchiveContainerNameForAddSSRFile", "testArchive" },
                {"ChannelIdToAddSSR", "test" },
                {"OrderMgt:ServiceBusConnectionString", "https://dummy.com/123/" },
                {"OrderMgt:ServiceBusTopicName", "testteventmessage" },
                {"OrderMgt:AppId", "123" },
                {"OrderMgt:ManageSpecialServiceResponseUrl", "http://localhost:7071/api/{0}/{1}" },
                {"FCStampingTableName", "TestTable" },
                {"CodeSharePartnerForStatementText", "VS,DL,AF,KL" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
