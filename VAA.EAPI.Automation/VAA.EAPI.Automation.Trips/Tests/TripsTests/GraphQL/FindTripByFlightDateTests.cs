using System.Globalization;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public sealed class FindTripByFlightDateTests : TripsGraphQlTestsBase, IClassFixture<CosmosFixture>
    {
        private readonly ScvHistoricTripDataSource _scvDataSource;

        public FindTripByFlightDateTests(CosmosFixture cosmosFixture)
        {
            _scvDataSource = new ScvHistoricTripDataSource(cosmosFixture, env);
        }

        [Fact]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        public async Task FindTripByFlightDate_TC01_PastScvBooking_ReturnsMatchingBooking()
        {
            ScvHistoricTripTestData testData = _scvDataSource.GetBooking();

            HttpResponseMessage response = GraphQlEndpoint.FindTripByFlightDate(
                    env,
                    testData.Pnr,
                    testData.FlightDate,
                    testData.GivenName,
                    testData.Surname)
                .EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();
            Assert.False(responseBody.IsNullOrWhiteSpace(), "FindTripByFlightDate response body was empty.");
            JObject result = JObject.Parse(responseBody);
            JArray bookings = (JArray)result.RequireToken("data.FindTripByFlightDate");
            Assert.NotEmpty(bookings);

            JObject? matchingBooking = bookings
                .Children<JObject>()
                .FirstOrDefault(booking => testData.OrderId.Is(booking.Value<string>("id")));
            Assert.NotNull(matchingBooking);

            string returnedPnr = matchingBooking.RequireToken("product[0].reference.pnrLocator").ToString();
            Assert.True(testData.Pnr.Is(returnedPnr), "The GraphQL response did not return the SCV booking reference used for the lookup.");
            Assert.False(matchingBooking.RequireToken("product[0].checkInEligible").Value<bool>());
            Assert.False(matchingBooking.RequireToken("product[0].accessibleTravelEligible").Value<bool>());

            JArray customers = (JArray)matchingBooking.RequireToken("passengers");
            Assert.Contains(customers.Children<JObject>(), customer =>
                testData.GivenName.Is(customer.Value<string>("firstName")) &&
                testData.Surname.Is(customer.Value<string>("lastName")));

            IEnumerable<JObject> segments = matchingBooking
                .RequireToken("product[0].slices")
                .Children<JObject>()
                .SelectMany(slice => slice.RequireToken("segments").Children<JObject>());
            Assert.Contains(segments, segment =>
                testData.MarketingFlightNumber.Is(segment.SelectToken("marketingCarrier.flightNumber")?.Value<string>()) &&
                testData.OriginAirportCode.Is(segment.SelectToken("origin.code")?.Value<string>()) &&
                testData.DestinationAirportCode.Is(segment.SelectToken("destination.code")?.Value<string>()));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        public async Task FindTripByFlightDate_TC02_TodayAndFutureDates_ReturnSuccessfulLookup(int days)
        {
            ScvHistoricTripTestData testData = _scvDataSource.GetBooking();
            string flightDate = DateTime.UtcNow.AddDays(days).ToString("ddMMMyy", CultureInfo.InvariantCulture).ToUpperInvariant();
            HttpResponseMessage response = GraphQlEndpoint.FindTripByFlightDate(env, testData.Pnr, flightDate, testData.GivenName, testData.Surname);
            response.EnsureGQLResponseSuccess();
            JObject result = JObject.Parse(await response.Content.ReadAsStringAsync());
            JArray bookings = (JArray)result.RequireToken("data.FindTripByFlightDate");
            Assert.InRange(bookings.Count, 0, 1);
        }

        [Theory]
        [InlineData("pnr")]
        [InlineData("givenName")]
        [InlineData("surname")]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        public async Task FindTripByFlightDate_TC03_NonMatchingLookup_ReturnsEmptyList(string nonMatchingField)
        {
            ScvHistoricTripTestData testData = _scvDataSource.GetBooking();
            string pnr = nonMatchingField.Is("pnr") ? "AUT404" : testData.Pnr;
            string givenName = nonMatchingField.Is("givenName") ? "AUTOMATION_NO_MATCH" : testData.GivenName;
            string surname = nonMatchingField.Is("surname") ? "AUTOMATION_NO_MATCH" : testData.Surname;

            HttpResponseMessage response = GraphQlEndpoint.FindTripByFlightDate(
                    env,
                    pnr,
                    testData.FlightDate,
                    givenName,
                    surname)
                .EnsureGQLResponseSuccess();

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);
            JArray bookings = (JArray)result.RequireToken("data.FindTripByFlightDate");
            Assert.Empty(bookings);
        }

        [Fact]
        [Trait("Category", Traits.TripDetailsMobileRegression)]
        public async Task FindTripByFlightDate_TC04_InvalidFlightDateFormat_ReturnsGraphQlError()
        {
            HttpResponseMessage response = GraphQlEndpoint.FindTripByFlightDate(
                env,
                "AUT404",
                "2025-08-31",
                "Test",
                "Passenger");

            string responseBody = await response.Content.ReadAsStringAsync();
            JObject result = JObject.Parse(responseBody);
            JArray errors = (JArray)result.RequireToken("errors");
            Assert.NotEmpty(errors);
            Assert.Contains(errors.Children<JObject>(), error =>
                error.Value<string>("message")?.Contains("ddMMMyy", StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}
