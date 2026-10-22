using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public static class CampaignTestHelpers
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                {"CampaignEnrolmentEventSubject", "CampaignEnrolmentDocEvent" },
                  {"MembershipChangeDocType", "MembershipChange" },
                {"TargetedMembershipEventSubject", "TargetedMembershipDocEvent" },
                { "CommandStatusLocation", "http://localhost:7071/api/{0}s/status/{1}"},
                {"ServiceBusTopicName", "customertransactioneventmessages"},
                {"ContactNoteDocType", "ContactNote"},
                {"ContactNoteEventSubject", "ContactNotes"},
                {"TargetedMembershipDocType", "TargetedMembership"},
                {"TransactionEventSubject", "Transaction" },
                {"TransactionDocType", "Transaction" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
