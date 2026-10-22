using System;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.CRM.Email.Tests
{
    public class EmailTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"RefundFailureSubject", "Refund refusal - Transaction Id : {0}"},
                {"RefundFailureTemplate", "SOVGEN-01"},
                {"RefundFailureFromAddress", "Paymentsdonotreply@fly.virgin.com" },
                {"RefundFailureToAddress", "eapi.support@fly.virgin.com" },
                {"RefundFailureCcAddress", "test" },
                {"ACMSessionToken", "TestToken" },
                {"ACMCommunicationChannel", "Testchannel" },
                {"RetryCounter","5" },
                {"DelayTime","0:0:0:0" },
                {"enableemailmsauthentication", "True" },
                {"InviteEmailLink", "Test" },
                {"InviteEmailTemplate","Test" },
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
