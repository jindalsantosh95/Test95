using Google.Apis.Gmail.v1;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Integrations;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom.PageObjects;
using VAA.EAPI.Automation.Search.Helpers;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using Config = VAA.EAPI.Automation.Core.Config;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom
{
    public class LoggedInAwardSearch : IDisposable
    {
        private static readonly string env = Config.Environment;
        private static readonly string gmailCredentials = Vault.Default.GetSecretValue("Automation-Gmail-Credentials");
        private static readonly string gmailUsername = Vault.Default.GetSecretValue("Automation-Gmail-Account");
        private static readonly string gmailPassword = Vault.Default.GetSecretValue("Automation-Gmail-Password");

        private readonly string depatureDate = OfferSearchHelper.DepartureDate();
        private readonly TestConfig _testConfig;
        private readonly GmailService _gmailService;
        private readonly IWebDriver _webDriver;

        public LoggedInAwardSearch()
        {
            _testConfig = new TestConfig();
            _testConfig.Search.Passengers.AddPassengers(adults: 1, 0, 0, 0);
            if (Config.Environment.Is("prod"))
            {
                _gmailService = GmailIntegration.GetGmailService(gmailCredentials, out Exception ex);

                if (ex != null)
                    throw ex;

                _webDriver = GetBrowser(BrowserType.Chrome, "https://identity.virginatlantic.com/Identity.Virginatlantic.com/oauth2/v2.0/authorize?p=B2C_1A_VA_DIGITAL_SIGNUP_SIGNIN_CA&client_id=c95f4011-f15b-4bf4-9e51-4a66b93b1c2f&nonce=defaultNonce&redirect_uri=https%3A%2F%2Fwww.virginatlantic.com%2Fflying-club%2Faccount%2Foverview&scope=openid&response_type=id_token&prompt=login&response_mode=query", headless: true);
            }
        }

        [Fact]
        [Trait("Category", GlobalTraits.LocalRun)]
        [Trait("Tag", GlobalTraits.DotCom)]
        public void LoyaltyLoggedInAwardSearch()
        {
            if (Config.Environment.Is("uat"))
                return;

            _testConfig.Search.Origin = LocationType.LHR;
            _testConfig.Search.Destination = LocationType.JFK;
            _testConfig.Search.IsRewardSearch = true;
            _testConfig.Search.FareFamilyType = FareFamilyType.AWARDECONOMY;

            //Login to loyalty and extract Bearer token from the cookie
            new LoyaltyLogin(_webDriver).Login(gmailUsername, gmailPassword);

            string verificationCode = GmailIntegration.GetLatestEmailSnippet(_gmailService);

            new LoyaltyLogin(_webDriver).VerifyCode(verificationCode);
            new LoyaltyLogin(_webDriver).WaitForAccountOverview();

            System.Threading.Thread.Sleep(3000);
            var cookies = _webDriver.Manage().Cookies.AllCookies;
            _testConfig.Internal.Bearer = cookies.FirstOrDefault(cookie => cookie.Name == "com.virginatlantic.my-account.token")?.Value!;

            //Search and Summary offer
            GraphQLHelper.OfferSearchGraphQL(_testConfig);
        }

        public void Dispose()
        {
            // Clean up
            _webDriver.DriverCleanUp();
        }
    }
}