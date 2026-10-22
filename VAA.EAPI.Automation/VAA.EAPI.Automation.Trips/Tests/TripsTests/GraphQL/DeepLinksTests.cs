using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.Helpers.StorageAccount;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public class DeepLinksTests
    {
        public string env = Config.Environment;
        public static string ocpSubKey = Vault.Default.GetSecretValue("TestingTripsGraphQLSecureSubscriptionKey");


        [SkippableTheory]
        [Trait("Category", Traits.DeepLinksMobileRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        [InlineData(DeepLinks.TravelInsurance)]
        [InlineData(DeepLinks.FlyReady)]
        [InlineData(DeepLinks.FlightCancel)]
        [InlineData(DeepLinks.FlightChange)]
        [InlineData(DeepLinks.IROP_ViewDetails)]
        [InlineData(DeepLinks.IROP_FAF)]
        [InlineData(DeepLinks.POPSMeal)]
        [InlineData(DeepLinks.BaggageAllowance)]
        [InlineData(DeepLinks.FlightDisruption)]
        public async Task RetrieveBookingDeepLink_ReturnsExpectedUrl(DeepLinks deeplinkOption)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = JourneyType.OneWay.ToString();
            string reservationTypeString = ReservationType.REVENUE.ToString();
            string origin = "LHR";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, origin);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip to retrieve booking id and passenger ids and count of slices

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (DynamicHelper.HasProperty(BookingResult, "errors") ||
                !DynamicHelper.HasProperty(BookingResult, "data") ||
                !DynamicHelper.HasProperty(BookingResult.data, "Booking") ||
                BookingResult.data.Booking == null)
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: candidate PNR is not eligible for booking retrieval in current environment. {debugContext}");
                return;
            }

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            string bookingId = BookingResult.data.Booking.id;
            Assert.True(!string.IsNullOrEmpty(bookingId), $"Booking id should not be null or empty. {debugContext}");

            // validate if the pos - country code is present in the response and is either GB or US

            string posCountryCode = BookingResult.data.Booking.pos.countryCode;
            Assert.True(!string.IsNullOrEmpty(posCountryCode) && (posCountryCode == "GB" || posCountryCode == "US"), $"POS Country Code should not be null or empty and should be either GB or US. {debugContext}");

            #endregion

            #region - GraphQL call to retrieve the deep link for My Trips

            string deeplinkOptionString = deeplinkOption.ToString();
            string segmentId = deeplinkOptionString == "POPSMeal" ? "1" : null;

            HttpResponseMessage getDeepLinksResponse = GraphQlEndpoint.GetDeepLinksResponse(env, bookingId, deeplinkOptionString, segmentId).EnsureIsSuccessStatusCode();
            dynamic getDeepLinkssResult = JsonConvert.DeserializeObject<dynamic>(await getDeepLinksResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getDeepLinkssResult, "errors"), $"Expected a valid response from Graphql but found \n {getDeepLinkssResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(getDeepLinkssResult, "data"), $"Response does not contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(getDeepLinkssResult.data, "Deeplink"), $"Response should contain Deeplink property. {debugContext}");

            dynamic deepLinkDetails = getDeepLinkssResult.data.Deeplink;
            Assert.NotNull(deepLinkDetails);

            // Validate deeplinkUrl
            Assert.True(DynamicHelper.HasProperty(deepLinkDetails, "deeplinkUrl"), $"Deeplink should contain deeplinkUrl property. {debugContext}");
            string deeplinkUrl = deepLinkDetails.deeplinkUrl;
            Assert.True(!string.IsNullOrEmpty(deeplinkUrl), $"DeeplinkUrl should not be null or empty. {debugContext}");
            Assert.True(deeplinkUrl.StartsWith("https://"), $"DeeplinkUrl should start with https://. {debugContext}");

            // Validate deeplinkOption
            Assert.True(DynamicHelper.HasProperty(deepLinkDetails, "deeplinkOption"), $"Deeplink should contain deeplinkOption property. {debugContext}");
            string deeplinkOptionFromResponse = deepLinkDetails.deeplinkOption;
            Assert.True(deeplinkOptionFromResponse == deeplinkOptionString, $"Expected deeplinkOption to be {deeplinkOptionString} but got {deeplinkOptionFromResponse}. {debugContext}");


            #endregion
        }

        [SkippableFact]
        [Trait("Category", Traits.DeepLinksMobileRegression)]
        public async Task BidForUpgradeDeepLink_ReturnsExpectedUrl()
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = JourneyType.OneWay.ToString();
            string reservationTypeString = ReservationType.REVENUE.ToString();
            string origin = "LHR";
            string flightType = "VS";
            string paxCountString = "1";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, origin, paxCountString, flightType, "EconomyClassic");

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip to retrieve booking id and passenger ids and count of slices

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (DynamicHelper.HasProperty(BookingResult, "errors") ||
                !DynamicHelper.HasProperty(BookingResult, "data") ||
                !DynamicHelper.HasProperty(BookingResult.data, "Booking") ||
                BookingResult.data.Booking == null)
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: candidate PNR is not eligible for booking retrieval in current environment. {debugContext}");
                return;
            }

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            string bookingId = BookingResult.data.Booking.id;
            Assert.True(!string.IsNullOrEmpty(bookingId), $"Booking id should not be null or empty. {debugContext}");

            // validate if the pos - country code is present in the response and is either GB or US

            string posCountryCode = BookingResult.data.Booking.pos.countryCode;
            Assert.True(!string.IsNullOrEmpty(posCountryCode) && (posCountryCode == "GB" || posCountryCode == "US"), $"POS Country Code should not be null or empty and should be either GB or US. {debugContext}");

            #endregion

            #region - GraphQL call to retrieve the deep link for My Trips

            string deeplinkOptionString = "BidForUpgrade";
            string segmentId = null;

            HttpResponseMessage getDeepLinksResponse = GraphQlEndpoint.GetDeepLinksResponse(env, bookingId, deeplinkOptionString, segmentId).EnsureIsSuccessStatusCode();
            dynamic getDeepLinkssResult = JsonConvert.DeserializeObject<dynamic>(await getDeepLinksResponse.Content.ReadAsStringAsync());

            // Handle BidForUpgrade specific scenarios - return success link
            if (DynamicHelper.HasProperty(getDeepLinkssResult, "errors"))
            {
                string errorsJson = getDeepLinkssResult.errors.ToString();
                bool isPlusgradeDown = errorsJson.Contains("G009", StringComparison.OrdinalIgnoreCase)
                    || (errorsJson.Contains("500", StringComparison.OrdinalIgnoreCase)
                        && errorsJson.Contains("Source system", StringComparison.OrdinalIgnoreCase));
                string failMessage = isPlusgradeDown
                    ? $"BidForUpgrade deeplink failed — Plusgrade API appears to be down (G009 / 500 Internal Server Error from source system). Please investigate the Plusgrade service. {debugContext} Response:\n {getDeepLinkssResult}"
                    : $"Expected a valid response from Graphql but found \n {getDeepLinkssResult}. {debugContext}";
                Assert.Fail(failMessage);
            }

            Assert.True(DynamicHelper.HasProperty(getDeepLinkssResult, "data"), "Response does not contain data property");
            Assert.True(DynamicHelper.HasProperty(getDeepLinkssResult.data, "Deeplink"), "Response should contain Deeplink property");

            dynamic deepLinkDetails = getDeepLinkssResult.data.Deeplink;
            Assert.NotNull(deepLinkDetails);

            // Validate deeplinkUrl
            Assert.True(DynamicHelper.HasProperty(deepLinkDetails, "deeplinkUrl"), "Deeplink should contain deeplinkUrl property");
            string deeplinkUrl = deepLinkDetails.deeplinkUrl;
            Assert.True(!string.IsNullOrEmpty(deeplinkUrl), "DeeplinkUrl should not be null or empty");
            Assert.True(deeplinkUrl.StartsWith("https://"), "DeeplinkUrl should start with https://");

            // Validate deeplinkOption
            Assert.True(DynamicHelper.HasProperty(deepLinkDetails, "deeplinkOption"), "Deeplink should contain deeplinkOption property");
            string deeplinkOptionFromResponse = deepLinkDetails.deeplinkOption;
            Assert.True(deeplinkOptionFromResponse == deeplinkOptionString, $"Expected deeplinkOption to be {deeplinkOptionString} but got {deeplinkOptionFromResponse}");

            #endregion
        }

        [SkippableTheory]
        [Trait("Category", Traits.DeepLinksMobileRegression)]
        [Trait("Category", GlobalTraits.MobileAppsSanityTests)]
        [InlineData(DeepLinks.FlightCancel)]
        [InlineData(DeepLinks.FlightChange)]
        [InlineData(DeepLinks.IROP_ViewDetails)]
        [InlineData(DeepLinks.IROP_FAF)]
        [InlineData(DeepLinks.FlyReady)]
        public async Task SecureTripsDeepLink_ReturnsExpectedUrl(DeepLinks deeplinkOption)
        {
            #region - pre-requisite for fetching the PNR from the storage table

            string journeyTypeString = JourneyType.OneWay.ToString();
            string reservationTypeString = ReservationType.REVENUE.ToString();
            string origin = "LHR";

            Dictionary<string, string> pnrDetails = RetrieveFromStorageTable.RetrievePNRDetailsFromStorageTableForCurrentWeek(journeyTypeString, reservationTypeString, origin);

            ValidatePnrDetails(pnrDetails);

            string firstName = pnrDetails["firstName"];
            string lastName = pnrDetails["lastName"];
            string pnr = pnrDetails["pnr"];
            int paxCount = int.Parse(pnrDetails["passengersCount"]);
            string loyaltyNum = pnrDetails["flyingClubNumber"];
            origin = pnrDetails["origin"];
            string destination = pnrDetails["destination"];
            string debugContext = RequestHelper.FormatTripsBookingContext(pnr, firstName, lastName);

            Assert.True(!string.IsNullOrEmpty(pnr) && !string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && paxCount > 0, $"PNR, First Name, and Last Name should not be null or empty and Pax Count > 0. {debugContext}");

            #endregion

            #region - GraphQL call for retrieve trip to retrieve booking id and passenger ids and count of slices

            string ticketNum = string.Empty; // as we are performing a PNR Search
            string usingString = "PNR";

            HttpResponseMessage response = GraphQlEndpoint.Booking(env, usingString, pnr, firstName, lastName).EnsureIsSuccessStatusCode();
            dynamic BookingResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            if (DynamicHelper.HasProperty(BookingResult, "errors") ||
                !DynamicHelper.HasProperty(BookingResult, "data") ||
                !DynamicHelper.HasProperty(BookingResult.data, "Booking") ||
                BookingResult.data.Booking == null)
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped: candidate PNR is not eligible for booking retrieval in current environment. {debugContext}");
                return;
            }

            Assert.True(!DynamicHelper.HasProperty(BookingResult, "errors"), $"Expected a valid response from Graphql but found \n {BookingResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(BookingResult, "data"), $"Response should contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(BookingResult.data, "Booking"), $"Response should contain Booking property. {debugContext}");

            string bookingId = BookingResult.data.Booking.id;
            Assert.True(!string.IsNullOrEmpty(bookingId), "Booking id should not be null or empty");

            // validate if the pos - country code is present in the response and is either GB or US

            string posCountryCode = BookingResult.data.Booking.pos.countryCode;
            Assert.True(!string.IsNullOrEmpty(posCountryCode) && (posCountryCode == "GB" || posCountryCode == "US"), "POS Country Code should not be null or empty and should be either GB or US");

            #endregion

            #region - GraphQL call to retrieve the deep link for My Trips

            string deeplinkOptionString = deeplinkOption.ToString();
            string segmentId = null;

            HttpResponseMessage getDeepLinksResponse = GraphQlEndpoint.GetDeepLinksResponse(env, bookingId, deeplinkOptionString, segmentId).EnsureIsSuccessStatusCode();
            dynamic getDeepLinkssResult = JsonConvert.DeserializeObject<dynamic>(await getDeepLinksResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getDeepLinkssResult, "errors"), $"Expected a valid response from Graphql but found \n {getDeepLinkssResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(getDeepLinkssResult, "data"), $"Response does not contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(getDeepLinkssResult.data, "Deeplink"), $"Response should contain Deeplink property. {debugContext}");

            dynamic deepLinkDetails = getDeepLinkssResult.data.Deeplink;
            Assert.NotNull(deepLinkDetails);

            // Validate deeplinkUrl
            Assert.True(DynamicHelper.HasProperty(deepLinkDetails, "deeplinkUrl"), $"Deeplink should contain deeplinkUrl property. {debugContext}");
            string deeplinkUrl = deepLinkDetails.deeplinkUrl;
            Assert.True(!string.IsNullOrEmpty(deeplinkUrl), $"DeeplinkUrl should not be null or empty. {debugContext}");

            // Validate deeplinkOption
            string deeplinkOptionFromResponse = deepLinkDetails.deeplinkOption;
            Assert.True(deeplinkOptionFromResponse == deeplinkOptionString, $"Expected deeplinkOption to be {deeplinkOptionString} but got {deeplinkOptionFromResponse}. {debugContext}");

            #endregion

            #region - call for deeplinkSecure URL with cache id for trips to validate the response

            // Extract cache ID from deeplinkUrl
            string cacheId = string.Empty;
            if (deeplinkUrl.Contains("id="))
            {
                Uri uri = new(deeplinkUrl);
                System.Collections.Specialized.NameValueCollection query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                cacheId = query["id"];
            }

            Assert.True(!string.IsNullOrEmpty(cacheId), $"Cache ID should not be null or empty. {debugContext}");

            HttpResponseMessage getDeepLinksSecureResponse = GraphQlEndpoint.GetDeepLinksSecureResponse(env, cacheId, ocpSubKey).EnsureIsSuccessStatusCode();
            dynamic getDeepLinksSecureResult = JsonConvert.DeserializeObject<dynamic>(await getDeepLinksSecureResponse.Content.ReadAsStringAsync());

            Assert.True(!DynamicHelper.HasProperty(getDeepLinksSecureResult, "errors"), $"Expected a valid response from Graphql but found \n {getDeepLinksSecureResult}. {debugContext}");

            Assert.True(DynamicHelper.HasProperty(getDeepLinksSecureResult, "data"), $"Response does not contain data property. {debugContext}");
            Assert.True(DynamicHelper.HasProperty(getDeepLinksSecureResult.data, "GetDeepLinkForPost"), $"Response should contain GetDeepLinkForPost property. {debugContext}");

            dynamic getDeepLinkForPostDetails = getDeepLinksSecureResult.data.GetDeepLinkForPost;
            Assert.NotNull(getDeepLinkForPostDetails);

            // Validate endpoint property
            Assert.True(DynamicHelper.HasProperty(getDeepLinkForPostDetails, "endpoint"), $"GetDeepLinkForPost should contain endpoint property. {debugContext}");

            dynamic endpointDetails = getDeepLinkForPostDetails.endpoint;
            Assert.NotNull(endpointDetails);

            // Validate endpointUrl
            string endpointUrl = endpointDetails.endpointUrl;
            Assert.True(!string.IsNullOrEmpty(endpointUrl), $"EndpointUrl should not be null or empty. {debugContext}");
            Assert.True(endpointUrl.StartsWith("https://"), $"EndpointUrl should start with https://. {debugContext}");

            // Validate http_Method
            string httpMethod = endpointDetails.http_Method;
            Assert.True(httpMethod == "POST", $"Expected http_Method to be POST but got {httpMethod}. {debugContext}");

            #endregion
        }

        private void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);

    }
}
