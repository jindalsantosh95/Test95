using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Functions;
namespace VAA.EAPI.Microservices.Loyalty.Wallet.Tests
{
    public class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            AsyncContext.Current = new AsyncContext() { Context = new TestFunctionContext() };
            Dictionary<string, string> settings = new()
            {
                {"AirshipFlightEndpoint","https://wallet-api.asnapieu.com/v1/flights/project/id/boarding_pass/id"},
                {"AirshipWalletLinkEndpoint","https://wallet-api.asnapieu.com/v1/links/adaptive/multiple/project/217"},
                {"AirshipTokenValue", "Test-Value"},
                {"RedisConnectionString", "Test-connection-string-value" },
                {"BoardingPassSlidingExpiryHours", "48"},
                {"SecureProgramImageUrl", "https://Test.Test.com/boarding-pass-badges.png"},
                {"SkyPriorityImgUrl", "http://Test.Test.com/content/sky-priority-badge.png"},
                {"TSAPreImgUrl", "http://Test.Test.com/content/tsa-pre-badge.png" },
                {"AirshipIosPremiumTemplate","334660"},
                {"AirshipIosUpperTemplate","Test-2" },
                {"AirshipIosDLTemplate", "Test-3" },
                {"AirshipIosOATemplate", "Test-4" },
                {"AirshipAndroidPremiumTemplate", "336105"},
                {"AirshipAndroidUpperTemplate", "Test-6"},
                {"AirshipAndroidDLTemplate", "Test-7"},
                {"AirshipAndroidOATemplate", "Test-8"},
                {"AirshipIosEconomyTemplate", "Test-9"},
                {"AirshipAndroidEconomyTemplate", "Test-10" },
                {"AirShipWalletAdaptivePassUrlPrefix", "https://wallet-api.asnapieu.com/v1/pass/adaptive" },
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
