using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Tests
{
    public static class ContactNoteTestHelpers
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                {"NotesDocType","ContactNote" },
                {"AwardDocType", "Award" },
                {"TopMemberProductID", "1634561747" },
                {"ContactNoteEventSubject", "LoyaltyNotes" },
                {"ContactNoteDocType", "ContactNote"},
                { "CommandStatusLocation", "http://localhost:7071/api/{0}s/status/{1}"},
                {"ServiceBusTopicName", "customertransactioneventmessages"},
                {"TransactionDBEndPoint","https://avadeunkveapis.vault.azure.net/secrets/AVADEUNAFNCUSTTRANSSMS-Config-TransactionDBEndPoint/" },
                {"TransactionDB", "CustomerTransactionsDB_NonProd"},
                {"TransactionCollection", "CustomerDetailsCollection_Test"},
                {"FlyingClubMemberDocType", "FlyingClubMember"},
                {"GroupDocType", "Group"},
                {"TargetedMembershipDocType", "TargetedMembership"},
                {"TargetedMembershipEventSubject", "TargetedMembership"},
                {"TransactionDocType", "Transaction"},
                {"MembershipChangeDocType", "MemberShipChange" },
                {"PartnerMembershipDocType", "PartnerMembership"},
                {"PartnerMembershipEventSubject", "PartnerMembership"},
                {"AuditServiceBusTopicName","AuditServiceBusTopicName" },
                {"flyingclubmembereventsubject","flyingclubmembereventsubject" },
                {"commandstatuspollingdelayms","100" },
                {"CommandStatusPollingRetryCount","10" },
                {"awardeventsubject","awardeventsubject" },
                {"TransactionEventSubject", "Transaction" },
                {"microservicedbkey", "xxx"},
                {"microserviceeventstoretablename", "test" },
                {"ServiceBusConnectionString", "https://dummy.com/123/" },
                {"BlobNameForStatementTxt", "StatementTextValues.json" },
                {"LoyaltyMemberMgmtStorageSASKey", "xxx"},
                {"ContainerNameForStatementTxt", "testcontainer" },
                {"BlobConnStringForStatementTxt", "https://dummy.com/123/" },
                {"MicroServiceResponseTableName", "test" },
                {"VredServiceBusTopicName", "testtopic" },
                {"VredCommandStatusLocation", "http://testvred/api/{0}"},
                {"CacheExpiryHours", "1" },
                {"TransactionRecentWindowHours", "1"},
                {"VoucherMonitoringDurationMinutes", "15" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
