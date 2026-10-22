using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.PageObjects.DotCom;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using VAA.EAPI.Automation.Trips.Requests.Endpoint;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.Microservices
{
    public class DeeplinkTests : IDisposable
    {
        private readonly IWebDriver _driver;
        private readonly HomePage _homePage;

        public static string env = Config.Environment;
        public static string ocpSubKey = Vault.Default.GetSecretValue("TestingTripMicroserviceSubscriptionKey");

        public DeeplinkTests()
        {
            _driver = GetBrowser(BrowserType.Chrome, "");
            _homePage = new HomePage(_driver);
        }

        [Theory]
        [Trait("Category", Traits.DeepLinksMSRegression)]
        [Trait("Category", Traits.LocalExecution)]
        [InlineData("TravelInsurance", "GB")]
        [InlineData("TravelInsurance", "US")]
        [InlineData("IROP_ViewDetails", "GB")]
        public async Task DeeplinkTests_Validation_Success(string deeplinkOption, string posCountryCode)
        {
            string journeyTypeString = JourneyType.OneWay.ToString();
            string reservationTypeString = ReservationType.REVENUE.ToString();
            Dictionary<string, string> pnrDetails = null;

            if (posCountryCode.Is("GB"))
            {
                pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, "LHR");
            }
            else if (posCountryCode.Is("US"))
            {
                pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, "JFK");
            }

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            string origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, "PNR, First Name, and Last Name should not be null or empty and Pax Count > 0");

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}");

            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), "Response should contain data property");

            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), "Response should contain Booking property");

            string bookingId = BookingResult.data.Booking.id;
            Assert.True(!string.IsNullOrEmpty(bookingId), "Booking id should not be null or empty");

            //Microservice call using the bookingId from GQL
            string deeplinkRequest = RequestHelper.DeeplinkRequest(bookingId, deeplinkOption, posCountryCode);

            HttpResponseMessage deeplinkResponse = Microservice.Deeplink(deeplinkRequest, ocpSubKey).EnsureIsSuccessStatusCode();

            JObject deeplinkJobjectResponse = JsonConvert.DeserializeObject<JObject>(await deeplinkResponse.Content.ReadAsStringAsync());

            string deeplinkUrl = deeplinkJobjectResponse["deeplinkUrl"].ToString();

            _driver.Navigate().GoToUrl(deeplinkUrl);

            if (deeplinkOption == "IROP_ViewDetails")
            {
                Uri uri = new(deeplinkUrl);
                System.Collections.Specialized.NameValueCollection queryParams = HttpUtility.ParseQueryString(uri.Query);
                string id = queryParams["id"];

                //Check the cache using the get call to the MS
                HttpResponseMessage deeplinkResponseFromCache = Microservice.GetCachedDeeplink(id, ocpSubKey).EnsureIsSuccessStatusCode();

                JObject deeplinkResponseFromCacheAsJObject = JsonConvert.DeserializeObject<JObject>(await deeplinkResponseFromCache.Content.ReadAsStringAsync());

                string returnedPnr = deeplinkResponseFromCacheAsJObject["parameters"][0]["paramValue"].ToString();

                Assert.True(returnedPnr == pnr, $"PNRs were not matching as expected. Expceted {pnr} and returned {returnedPnr}");

                _driver.WaitForElementsToBeVisible("//button[contains(text(),'Yes, I Agree')]", 60);

                _homePage.Click("Yes, I Agree");

                _driver.WaitForElementsToBeVisible("//section[@class='td-flight-route']");

                string pageHtml = _driver.PageSource;

                Assert.True(pageHtml.Has(pnr), $"PNR {pnr} was not found in the page");
            }
            if (deeplinkOption.Is("TravelInsurance"))
            {
                if (posCountryCode.Is("GB"))
                {
                    _homePage.Click("Accept All Cookies");

                    string url = _driver.Url;

                    Assert.True(url.Has("magroup-online.com/WL/VAA/GB/en"));

                    string pageHtml = _driver.PageSource;

                    Assert.True(pageHtml.Has("https://www.virginatlantic.com"), $"URL to Virgin Atlantic was not found in the deep link page");
                }
                if (posCountryCode.Is("US"))
                {
                    _homePage.Click("Accept All");

                    string url = _driver.Url;

                    Assert.True(url.Has("etravelprotection.com/virginatlantic/home"));

                    string pageHtml = _driver.PageSource;

                    Assert.True(pageHtml.Has("https://www.virginatlantic.com"), $"URL to Virgin Atlantic was not found in the deep link page");
                }
            }
        }

        private static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
        {
            Assert.False(pnrDetails.ContainsKey("error"), pnrDetails.ContainsKey("error") ? $"Test case failed: {pnrDetails["error"]}" : string.Empty);

            Assert.True(!string.IsNullOrEmpty(pnrDetails["pnr"]) && !string.IsNullOrEmpty(pnrDetails["firstName"]) && !string.IsNullOrEmpty(pnrDetails["lastName"]) && int.Parse(pnrDetails["passengersCount"]) > 0, "PNR, First Name, and Last Name should not be null or empty and Pax Count > 0");
        }

        public void Dispose() => _driver.DriverCleanUp();
    }
}
