using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;

namespace VAA.EAPI.Automation.Core
{
    public static class Config
    {
        public static readonly string Environment = ConfigReader.Get("env") ?? "";

        public static readonly string Version = ConfigReader.Get("ApiVersion") ?? "";

        public static readonly string FrontEndBasketApi = ConfigReader.Get("FrontEndBasketApi") ?? "";

        public static readonly bool UseDigitalNdcEndpoints = ConfigReader.Try("UseDigitalNdcEndpoints", false);

        public static readonly string DigitalNdcBaseUrl = ConfigReader.Get("DigitalNdcBaseUrl") ?? "";

        public static string DigitalNdcOfferManagementBaseUrl
        {
            get
            {
                if (DigitalNdcBaseUrl.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("DigitalNdcBaseUrl must be configured when UseDigitalNdcEndpoints is enabled.");

                if (Environment.IsNullOrWhiteSpace())
                    throw new InvalidOperationException("env must be configured when UseDigitalNdcEndpoints is enabled.");

                return $"{DigitalNdcBaseUrl.TrimEnd('/')}/{Environment.Trim().Trim('/')}/Digital/SalesAndRevenueManagement/OrderShopping/OfferManagement";
            }
        }

        public static class ScvHistoricTrips
        {
            public static readonly string AccountEndpoint = ConfigReader.Get("ScvAccountEndpoint");
            public static readonly string Database = ConfigReader.Get("ScvDatabase");
            public static readonly string Collection = ConfigReader.Get("ScvCollection");
        }

        public static class TestResourcesVirginAtlantic
        {
            public static readonly string BaseUrl = ConfigReader.Get("BaseUrl");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}".ReplaceIf(Environment.Is("prod"), "testresources", "resources");
        }

        public static class DigitalTestVirginAtlantic
        {
            public static readonly string BaseUrlDigital = ConfigReader.Get("BaseUrlDigital");

            public static string GetUrl(string resource) => $"{BaseUrlDigital.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}".ReplaceIf(Environment.Is("prod"), "digital-test", "digital");
        }

        public static class TestResourcesLoyaltyMiles
        {
            public static readonly string BaseUrl = ConfigReader.Get("eie_loyaltyMiles");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}".ReplaceIf(Environment.Is("prod"), "testresources", "resources");
        }

        public static class NdcDeveloper
        {
            public static readonly string BaseUrl = ConfigReader.Get("BaseUrl");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class MicrosoftOnline
        {
            public static readonly string BaseUrl = $"https://login.microsoftonline.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class Avateunwaorderms
        {
            public static readonly string BaseUrl = "https://avateunwaorderms.azurewebsites.net";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class Crm
        {
            public static readonly string BaseUrl = "https://vaauat.api.crm4.dynamics.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class Soa
        {
            public static readonly string BaseUrl = "https://webservices.vs01.vs.air4.com:31060";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SoaSeatV8
        {
            public static string soaProd = "https://soaws.vs.air4.com:12000/seat/v8";
            public static string soaStg = "https://soaws.vs01.vs.air4.com:12000";

            public static string BaseUrl = soaStg;

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SoaPassenger
        {
            public static readonly string BaseUrl = "https://webservices.vs01.vs.air4.com:31057";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SoaBrand
        {
            public static readonly string BaseUrl = "https://stage-pvt-apigateway.vs01.vs.air4.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class VredEndPoint
        {
            public static readonly string BaseUrl = Environment.Equals("uat", StringComparison.InvariantCultureIgnoreCase) ? "https://red-api-r1.model.loyalty.virgin.com" : "https://red-api-r1.staging.loyalty.virgin.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class VredTokenCreationEndpoint
        {
            public static readonly string BaseUrl = Environment.Equals("uat", StringComparison.InvariantCultureIgnoreCase) ? "https://idp.b2b.model.red.virgin.com" : "https://idp.b2b.staging.red.virgin.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class AzureB2C
        {
            public static readonly string BaseUrl = "https://graph.microsoft.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SubscriptionKey
        {
            //public static readonly string BaseUrl = "https://avadeunafneapisecurity.azurewebsites.net";
            public static readonly string BaseUrl = "https://testresources.virginatlantic.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class AtpcoGetToken
        {
            public static readonly string BaseUrl = $"https://gold.apis.atpco.net";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class VredProfileEndPoint
        {
            public static readonly string BaseUrl = "https://b2b.staging.red.virgin.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class DeltaEndPoint
        {
            public static readonly string BaseUrl = "https://stage-pvt-apigateway.vs01.vs.air4.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class AwsDeltaEndPoint
        {
            public static readonly string BaseUrl = "https://offer-api.vs-pax-ofr-si.aws.vs.air4.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SoaForZuluEnvt
        {
            public static readonly string BaseUrl = ConfigReader.Get("air4_acceptance");

            public static string GetUrl(string resource) => $"{BaseUrl}:{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SeatForZuluEnvt
        {
            public static readonly string BaseUrl = ConfigReader.Get("air4_seatacceptance");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class EIELayerForZuluEnvt
        {
            public static readonly string BaseUrl = ConfigReader.Get("eie_acceptance");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class EIELayerForZuluEnvt_loyaltyMiles
        {
            public static readonly string BaseUrl = ConfigReader.Get("eie_loyaltyMiles");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class RestForZuluEnvt
        {
            public static readonly string BaseUrl = ConfigReader.Get("rest_acceptance");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class ChooseAPIEndPoint
        {
            public static readonly string BaseUrl = "https://partner-test.api.chooose.today";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SoaForLoopsAIR4
        {
            public static readonly string BaseUrl = $"https://webservices.vs01.vs.air4.com:31057";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class SoaForLoyaltyProfileSyncAIR4
        {
            public static readonly string BaseUrl = $"http://webservices.vs01.vs.air4.com:32031";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class DuplicateReceiptForZuluEnvt
        {
            public static readonly string BaseUrl = ConfigReader.Get("eie_duplicateReceipt");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class GetBookedTotalsForZuluEnvt
        {
            public static readonly string BaseUrl = ConfigReader.Get("getBookedTotals");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class BaggageForZuluEnvt
        {
            public static readonly string BaseUrl = ConfigReader.Get("air4_baggageacceptance");

            public static string GetUrl(string resource) => $"{BaseUrl}:{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class DeltaPCCOciWebEndPoint
        {
            public static readonly string BaseUrl = "https://stg.virginatlantic.com/PCCOciWeb/apiv2/auth";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class DeltaTRMAPIEndPoint
        {
            public static readonly string BaseUrl = "https://travelrequirementsmgt-api-si.vs01.vs.air4.com";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class DeltaPaymentManagementEndPoint
        {
            public static readonly string BaseUrl = "https://stage-snsapigateway2.vs01.vs.air4.com/paymentManagement/v1";

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class DeltaHostedPaymentEndPoint
        {
            public static readonly string BaseUrl = "https://enterprisepaymentshpp-si.vs01.vs.air4.com/hpp";

            public static string GetUrl(string paymentReferenceId) =>
                $"{BaseUrl}?paymentReferenceId={Uri.EscapeDataString(paymentReferenceId)}&channelId=DFMOBILE&appId=D2&purchasePath=DFMOBILEOCI";
        }

        #region - My trips configuration

        public static class AIR4ResourcesMyTrips
        {
            public static readonly string BaseUrl = ConfigReader.Get("AIR4ResourcesMyTrips");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class AIR4ResourcesRetrieveSSRs
        {
            public static readonly string BaseUrl = ConfigReader.Get("AIR4ResourcesRetrieveSSRs");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        public static class AIR4ResourcesMessageCenter
        {
            public static readonly string BaseUrl = ConfigReader.Get("AIR4ResourcesMessageCenterAPI");

            public static string GetUrl(string resource) => $"{BaseUrl.TrimEnd('/')}/{(resource ?? string.Empty).TrimStart('/')}";
        }

        #endregion - My trips configuration
    }
}
