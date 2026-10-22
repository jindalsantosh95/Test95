using System.Net;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.Requests;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class BackendBaggageTests
{
    private static readonly FlightSearch Search = new("LHR", "JFK", new DateOnly(2027, 1, 10), PassengerCounts.OneAdult);
    private const string Summary = """
        {"price":{"currency":"GBP"},"flights":[{"flightId":"journey-1","fareFamilyType":"ECON","cabinName":"Economy Light","segments":[
          {"origin":{"code":"LHR"},"destination":{"code":"JFK"},"departure":"2027-01-10T10:00:00","arrival":"2027-01-10T13:00:00",
           "operatingAirline":{"code":"VS","name":"Virgin Atlantic"},"flightNumber":"VS3","stopCount":0}]}],
         "flightPassengerBaggage":[{"flightRefId":"journey-1","carryOnBag":{"passengerBaggageAllowance":{
           "freeBaggageAllowance":{"allowedBagCnt":1,"weight":{"unitOfMeasureCnt":10,"unitOfMeasure":"KG"},
            "dimensionAllowance":{"height":"56","width":"36","length":"23","unitOfMeasure":"CM"}}}}}]}
        """;
    private const string SearchResponse = """
        {"result":{"slice":{"flightsAndFares":[{"flight":{"segments":[{"operatingAirline":{"code":"VS"},"stopCount":0}]},
        "fares":[{"fareFamilyType":"ECON","links":[{"rel":"summary","href":"https://testresources.virginatlantic.com/sit/search/11111111-1111-1111-1111-111111111111/summary/slice1.1/offer-1"}]}]}]}}}
        """;

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public async Task BackendFirst_PerformsSearchThenSummaryAndKeepsExpectationsInMemory()
    {
        using Handler handler = new(HttpStatusCode.OK);
        using BackendSearchClient client = new(Settings(), handler);
        BackendFareExpectation expected = await client.PrepareAsync(Search);
        Assert.Equal(new[] { HttpMethod.Post, HttpMethod.Get }, handler.Methods);
        Assert.Equal("/sit/search/search", handler.Paths[0]);
        Assert.Contains("/sit/search/summary/11111111-1111-1111-1111-111111111111/", handler.Paths[1]);
        Assert.Equal("Economy Light", expected.FareTitle);
        Assert.Contains("1 dimension comparisons", BackendBaggageAssertions.AssertMatches(expected.Baggage,
            ["1 x 10kg Carry-on bag Max dimensions 56 x 36 x 23cm"]));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Environment", "PROD")]
    [InlineData("Environment", "UAT")]
    [InlineData("BaseUrl", "http://testresources.virginatlantic.com")]
    [InlineData("BaseUrl", "https://resources.virginatlantic.com")]
    [InlineData("BaseUrl", "https://testresources.virginatlantic.com.evil.example")]
    [InlineData("BaseUrl", "https://testresources.virginatlantic.com/prod")]
    [InlineData("TimeoutSeconds", "61")]
    [InlineData("Enabled", "false")]
    public void UnsafeConfiguration_IsRejected(string key, string value)
        => Assert.Throws<InvalidOperationException>(() => new BackendSearchClient(Settings(key, value)));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(302)]
    [InlineData(401)]
    [InlineData(403)]
    [InlineData(500)]
    public async Task BackendFailure_DoesNotRetryOrExposeBody(int status)
    {
        using Handler handler = new((HttpStatusCode)status);
        using BackendSearchClient client = new(Settings(), handler);
        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => client.PrepareAsync(Search));
        Assert.Single(handler.Methods);
        Assert.Contains($"HTTP {status}", error.Message);
        Assert.DoesNotContain("sensitive-marker", error.ToString());
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public async Task Timeout_IsBoundedWithNoRetry()
    {
        using Handler handler = new(HttpStatusCode.OK, true);
        using BackendSearchClient client = new(Settings("TimeoutSeconds", "1"), handler);
        await Assert.ThrowsAsync<InvalidOperationException>(() => client.PrepareAsync(Search));
        Assert.Single(handler.Methods);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SearchCriteria_AreSharedAndNoMatchIsExplicit()
    {
        JObject payload = JObject.Parse(BackendSearchExpectation.SearchPayload(Search));
        Assert.Equal("2027-01-10", payload.SelectToken("slices[0].departure")?.Value<string>());
        Assert.Equal("ADULT", payload.SelectToken("passengers[0]")?.Value<string>());
        Assert.False(payload.Value<bool>("awardSearch"));
        Assert.Throws<InvalidOperationException>(() => BackendSearchExpectation.SelectEconomyFare(SearchResponse.Replace("VS", "KL")));
        Assert.Throws<InvalidOperationException>(() => BackendSearchExpectation.SearchPayload(Search with { Passengers = new PassengerCounts(2) }));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("GBP", "USD")]
    [InlineData("2027-01-10", "2027-01-11")]
    [InlineData("LHR", "MAN")]
    [InlineData("ECON", "UPPE")]
    public void BackendSummary_MustMatchSearchScope(string from, string to)
        => Assert.Throws<InvalidOperationException>(() => BackendSearchExpectation.ReadSummary(Summary.Replace(from, to), Search));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void UiMatch_RequiresExactScheduleCarrierRouteAndFlightNumber()
    {
        BackendFareExpectation expected = BackendSearchExpectation.ReadSummary(Summary, Search);
        FlightLegDetails actual = expected.Flight with { Descriptor = "VS3 - Airbus A350" };
        Assert.True(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails([actual], [])));
        Assert.False(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails([actual with { Descriptor = "VS30" }], [])));
        Assert.False(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails([actual with { DepartureTime = new TimeOnly(11, 0) }], [])));
        Assert.False(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails([actual with { OperatingCarrier = "Delta" }], [])));
        Assert.False(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails([actual with { Destination = "BOS" }], [])));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("1 x 10kg Carry-on bag Max dimensions 57 x 36 x 23cm")]
    [InlineData("2 x 10kg Carry-on bag Max dimensions 56 x 36 x 23cm")]
    [InlineData("1 x 12kg Carry-on bag Max dimensions 56 x 36 x 23cm")]
    [InlineData("1 x 10kg Checked-in bag Max dimensions 56 x 36 x 23cm")]
    [InlineData("1 x 10kg Carry-on bag")]
    public void BaggageComparison_RejectsIncorrectValues(string row)
        => Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => BackendBaggageAssertions.AssertMatches(BasketBaggageSnapshot.Read(Summary), [row]));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void Currency_MapsGbpToPoundSymbolWithoutAcceptingDollars()
    {
        BackendFlightMatching.AssertCurrency("GBP", "Select Economy Light £450.00");
        Assert.ThrowsAny<Xunit.Sdk.XunitException>(() => BackendFlightMatching.AssertCurrency("GBP", "Select Economy Light $450.00"));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void PartnerSelection_RequiresConnectionAndNonVirginOperator()
    {
        JObject response = JObject.Parse(SearchResponse);
        JArray segments = (JArray)Required(response, "result.slice.flightsAndFares[0].flight.segments");
        segments.Add(segments[0].DeepClone());
        Assert.Throws<InvalidOperationException>(() => BackendSearchExpectation.SelectEconomyFare(response.ToString(), connectingPartner: true));
        Required(segments[1], "operatingAirline")["code"] = "KL";
        Assert.Equal("slice1.1|offer-1", BackendSearchExpectation.SelectEconomyFare(response.ToString(), connectingPartner: true).FareId);
        Assert.Throws<InvalidOperationException>(() => BackendSearchExpectation.SelectEconomyFare(response.ToString()));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void PartnerSummary_MatchesEveryOrderedLegAndConnectionAirport()
    {
        BackendFareExpectation expected = BackendSearchExpectation.ReadSummary(PartnerSummary().ToString(), Search, connectingPartner: true);
        FlightLegDetails[] observed = expected.Legs.Select((leg, index) => leg with { Descriptor = expected.FlightNumbers[index] }).ToArray();
        Assert.True(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails(observed, ["AMS"])));
        Assert.False(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails(observed, ["CDG"])));
        Assert.False(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails(observed.Reverse().ToArray(), ["AMS"])));
        observed[1] = observed[1] with { Descriptor = "KL999" };
        Assert.False(BackendFlightMatching.Matches(expected, new ConnectingFlightDetails(observed, ["AMS"])));
        Assert.Single(expected.Baggage); // Allowance belongs to the journey, not one duplicate per leg.
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("origin.code", "CDG")]
    [InlineData("operatingAirline.code", "")]
    public void PartnerSummary_RejectsBrokenConnectionOrMissingCarrier(string path, string value)
    {
        JObject summary = PartnerSummary();
        JToken segment = Required(summary, "flights[0].segments[1]");
        ((JValue)Required(segment, path)).Value = value;
        Assert.Throws<InvalidOperationException>(() => BackendSearchExpectation.ReadSummary(summary.ToString(), Search, connectingPartner: true));
    }

    private static JObject PartnerSummary()
    {
        JObject summary = JObject.Parse(Summary);
        JArray segments = (JArray)Required(summary, "flights[0].segments");
        JToken second = segments[0].DeepClone();
        Required(segments[0], "destination")["code"] = "AMS";
        Required(segments[0], "operatingAirline")["code"] = "KL";
        Required(segments[0], "operatingAirline")["name"] = "KLM";
        segments[0]["flightNumber"] = "KL1000";
        Required(second, "origin")["code"] = "AMS";
        Required(second, "operatingAirline")["code"] = "DL";
        Required(second, "operatingAirline")["name"] = "Delta Air Lines";
        second["flightNumber"] = "DL4";
        second["departure"] = "2027-01-10T14:00:00";
        second["arrival"] = "2027-01-10T16:00:00";
        segments.Add(second);
        return summary;
    }

    private static JToken Required(JToken token, string path)
        => token.SelectToken(path) ?? throw new InvalidOperationException("Missing synthetic fixture field: " + path);

    private static IConfiguration Settings(string? key = null, string? value = null)
    {
        Dictionary<string, string?> settings = new()
        {
            ["IOSLaunch:BuildProfile"] = "sit_standard",
            ["BackendValidation:Enabled"] = "true",
            ["BackendValidation:Environment"] = "SIT",
            ["BackendValidation:BaseUrl"] = "https://testresources.virginatlantic.com",
            ["BackendValidation:TimeoutSeconds"] = "30"
        };
        if (key is not null)
            settings[$"BackendValidation:{key}"] = value;
        return new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
    }

    private sealed class Handler(HttpStatusCode status, bool delay = false) : HttpMessageHandler
    {
        internal List<HttpMethod> Methods { get; } = [];
        internal List<string> Paths { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Methods.Add(request.Method);
            Paths.Add(request.RequestUri?.AbsolutePath ?? "");
            if (delay)
                await Task.Delay(Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage(status) { Content = new StringContent(status != HttpStatusCode.OK ? "sensitive-marker" : Methods.Count == 1 ? SearchResponse : Summary) };
        }
    }
}
