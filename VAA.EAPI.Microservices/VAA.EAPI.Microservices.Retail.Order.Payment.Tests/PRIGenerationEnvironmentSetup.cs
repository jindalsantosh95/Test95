using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    public class PRIGenerationEnvironmentSetup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"DlPaymentApiGenerateRefIdUri", "https://stage-snsapigateway.vs01.vs.air4.com/paymentManagement/v1/paymentReferenceId" },
                {"DlPaymentobjectName", "ABCD" },
                {"DlPaymentApiApplicationId", "AB" },
                {"DlPaymentAuthenticationUrl", "https://authentcationurl" },
                {"DlPaymentApiFormsOfPaymentsUrl", "https://stage-snsapigateway.vs01.vs.air4.com/paymentManagement/v1/formsOfPayments" },
                {"DlPaymentApiChannelId", "DLChannel" },
                {"DlPaymentClientId", "DLClientId" },
                {"DlPaymentApiGetPaymentDetailsUrl", "https://stage-snsapigateway.vs01.vs.air4.com/paymentManagement/v1/paymentDetails/{paymentReferenceId}" },
                {"DlPaymentPurchasePath", "DLPurchasePath" },
                {"DlPaymentClientSecret", "DLSecret" },
                {"ReferenceDataMicroServiceDBKey", "abc" },
                {"commoncachingslidingexpirationinday", "1" },
                {"commoncachingabsoluteexpirationinday", "1" },
                {"dlformsofpaymentsgooglepay" , "GOOGLE_PAY"},
                {"dlgooglepay", "GOOGLEPAY" },
                {"mspfinancialinstaccountholdername", "VAA MSP" },
                {"mspfinancialinstcountrycode", "GB" },
                {"mspfinancialinstproviderid", "testid" },
                {"mspfinancialinstreturnurl", "mspfinancialinstreturnurl" },
                {"MobileViewPaymentUrl", "https://stg.mobilePayment.com/Bag" },
                {"Air4HppPaymentLink", "https://Testpaymentshpp-Test.com/hpp?paymentReferenceId={paymentReferenceId}" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
