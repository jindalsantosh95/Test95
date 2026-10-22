using Amazon.Runtime.Internal.Transform;
using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests
{
    public class SettingsHelper
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },
                {"SCV:CustDB:CustSearchColCntnr", "CustomerCollection" },
                {"SCV:CustDB:EndPoint", "https://avadeuncmdbcustomer.documents.azure.com:443/" },
                {"SCV:CustDB", "CustomerDB_NonProd"},
                {"SCV:CustDB:CustColCntnr", "test" },
                {"scv:custdb:endpoint", "test" },
                {"addressupdatetemplate", "TestTemplate" },
                {"CustMSUpdateResponseURL", "TestResponseURL" },
                {"ResponsePollingCount", "5" },
                {"ResponseDelayTimeInMS", "10" },
                {"enrolmentagelimit", "18" },
                {"CustomerSearchCollection", "CustomerSearch" },
                {"custdb:custsearchcolcntnrfortest","CustomerSearch" },
                { "SCV:CustTxnDB:MmbrPrfleContnr", "CustomerDetailsCollection" },
                { "SCV:CustTxnDB:EndPoint", "https://avadeuncmdbcustomertransactions.documents.azure.com:443/" },
                { "SCV:CustTxnDB", "CustomerTransactionsDB_Acc" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value); 
        }
    }
}
