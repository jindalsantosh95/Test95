using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Trip.Tests
{
    public class TripMSEnvironmentSetup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                 { "DlUpsellOfffersUri","https://mytrips-api-si.vs01.vs.air4.com/v1/upsell/upsell" },
                { "x-air4_mytrips_test","testkey"},
                { "TravelInsuranceDeeplinkUrl_GB", "https://www.magroup-online.com/WL/VAA/GB/en"},
                { "TravelInsuranceDeeplinkUrl_US", "https://www.etravelprotection.com/virginatlantic/home"},
                { "DeeplinkExpiryLimitInMinutes", "05" },
                { "RedisConnectionString", "localhost:6379,password=yourpassword,ssl=False,abortConnect=False" },
                { "BookingIdJwtKey", "01525c79-107e-458e-a238-718b33d9eb8c"},
                { "ManageMyBookingUrl", "https://testdeeplink.com/irop/details" },
                { "ManageMyHolidayUrl", "https://testdeeplink.com/irop/details" },
                { "ProxyDeeplinkURL", "https://proxydeeplinktest.com/deeplink?id={0}" },
                { "IROP_FAFURL", "https://testdeeplink.com/irop/faf" },
                { "deltaclientidvalue","testcliendidvalue"},
                { "DeltaClientSecret","DeltaClientSecretvalue"},
                { "DeltaAir4TokenEndpoint", "https://ssaasi.vs01.vs.air4.com/as/token.oauth2"},
                {"DlMessageCentrePrivateUri","https://mytrips-api-si.vs01.vs.air4.com/v1/upsell-auth/upsell/" },
                {"dlupsellofffersprivateuri","https://mytrips-api-si.vs01.vs.air4.com/v1/upsell-auth/upsell/" },
                { "DlMyTripsPrivateUri","https://mytrips-api-si.vs01.vs.air4.com/v1/mytrips-auth/travelreservations"},
                { "POPSMealUrl", "https://popsdeeplinktest.com/en.html?token={0}" },
                { "POPSTokenKey", "ABCD1234" },
                { "PlusGradeApiKey", "apikey_test"},
                { "PlusGradeApiUri","https://request-api.stage.plusgrade.com/v1/offer" },
                { "PlusGradePartnerId","partnerid" },
                { "DlMyTripsUri", "https://mytrips-api-si.vs01.vs.air4.com/v1/mytrips/travelreservations" },
                {"SwitchToPrivateEndpoints" , "true" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
