using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Tests
{
    public class LoyaltySyncTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"ApplicationId", "FC"},
                {"AppChannelName", "SIEBELLOYALTY"},
                {"TestLab", "TVTH"},
                {"EnablePSDSSync", "true"},
                {"EnrolmentChannelList", "WEB-MYA|APP-MYA|POWERAPP" },
                {"ServiceBusTopicName", "enrolcustomerTest"},
                {"EventTopicTypeCustomerSync", "CustomerSyncEventTest"},
                {"CommandStatusLocation","http://localhost:7075/api/SyncSFPD/status/{1}" },
                {"commandstatuspollingdelayms","100" },
                {"CommandStatusPollingRetryCount","10" },
                {"EventTopicTypPSDSSync", "enrolcustomerTest" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }
    }
}
