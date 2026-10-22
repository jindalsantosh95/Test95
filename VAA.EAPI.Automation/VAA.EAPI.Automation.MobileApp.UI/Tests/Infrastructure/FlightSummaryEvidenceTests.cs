using System.Reflection;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class FlightSummaryEvidenceTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("VS", "Virgin Atlantic")]
    [InlineData("KL", "KLM")]
    [InlineData("DL", "Delta Air Lines")]
    public void CarrierEvidence_HandlesSwiftUiIdentifierPropagation(string code, string carrier)
    {
        // Shape captured from the local iOS results tree on 28 September 2026.
        // Coordinates and dates omitted; every sibling inherited the same row ID.
        string source = $"""
            <App>
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0Carrier{code}" label="8h 20m" />
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0Carrier{code}" label="Direct" />
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0Carrier{code}" label="Departure airport LHR" />
              <Image type="XCUIElementTypeImage" name="NativeFlightResult0Carrier{code}" label="airlineLogo" />
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0Carrier{code}" label="{carrier} +1" />
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0Carrier{code}" label="Arrival airport JFK" />
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult1CarrierVS" label="Virgin Atlantic" />
            </App>
            """;
        Assert.Equal((code, carrier), NativeFlightSearchPage.ReadOperatingCarrier(source, "NativeFlightResult0"));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("NativeFlightResult1CarrierVS")]
    [InlineData("NativeFlightResult0CarrierVSExtra")]
    public void CarrierEvidence_DoesNotCompleteRowFromAnotherIdentifier(string arrivalIdentifier)
    {
        string source = $"""
            <App>
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0CarrierVS" label="Departure airport LHR" />
              <Text type="XCUIElementTypeStaticText" name="NativeFlightResult0CarrierVS" label="Virgin Atlantic" />
              <Text type="XCUIElementTypeStaticText" name="{arrivalIdentifier}" label="Arrival airport JFK" />
            </App>
            """;
        Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.ReadOperatingCarrier(source, "NativeFlightResult0"));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("", "LHR", "JFK")]
    [InlineData("London to New York", null, null)]
    [InlineData("London to New York", "LHR", null)]
    public void SummaryRoute_AcceptsLegacyWithoutItineraryOrRedesignedRoute(
        string itinerary, string? legacyOrigin, string? legacyDestination)
        => Assert.Equal(("LHR", "JFK"),
            NativeFlightSearchPage.ResolveSummaryRoute(itinerary, legacyOrigin, legacyDestination));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(null, null)]
    [InlineData("LHR", null)]
    public void SummaryRoute_RejectsMissingItineraryAndIncompleteLegacyRoute(string? origin, string? destination)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.ResolveSummaryRoute("", origin, destination));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("LHR to JFK", "LHR", "JFK", true)]
    [InlineData("London to New York", "LHR", "JFK", true)]
    [InlineData("JFK to LHR", "LHR", "JFK", false)]
    [InlineData("New York to London", "LHR", "JFK", false)]
    [InlineData("New York to London", "JFK", "LHR", true)]
    public void ResultsRoute_RejectsReversedEndpoints(string route, string origin, string destination, bool valid)
    {
        Action assertion = () => NativeFlightSearchPage.AssertResultsRouteOrder(route, origin, destination);
        if (valid)
        {
            assertion();
        }
        else
        {
            Assert.Throws<InvalidOperationException>(assertion);
        }
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Virgin Atlantic marketing text", null)]
    [InlineData("NativeFlightResult1CarrierVS Virgin Atlantic", null)]
    [InlineData("NativeFlightResult10CarrierVS", null)]
    [InlineData("NativeFlightResult0CarrierVSExtra", null)]
    [InlineData("NativeFlightResult0CarrierKL Virgin Atlantic marketing text", "KL")]
    [InlineData("NativeFlightResult0CarrierVS", "VS")]
    public void CarrierCode_RequiresExactScopedIdentifier(string cardText, string? expected)
        => Assert.Equal(expected, NativeFlightSearchPage.ReadOperatingCarrierCode("NativeFlightResult0", cardText));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("true", "0", true)]
    [InlineData("false", "1", true)]
    [InlineData(null, "1", true)]
    [InlineData(null, "true", true)]
    [InlineData("false", "0", false)]
    [InlineData("false", "false", false)]
    [InlineData(null, null, false)]
    public void FilterSourceState_IncludesNativeValueWhenSelectedIsAbsentOrFalse(string? selected, string? value, bool expected)
    {
        string selectedAttribute = selected is null ? string.Empty : $"selected='{selected}'";
        string valueAttribute = value is null ? string.Empty : $"value='{value}'";
        string source = $"<App><Button name='filter' {selectedAttribute} {valueAttribute} /></App>";
        Assert.Equal(expected, NativeFlightSearchPage.IsFilterSelectedInPageSource(source, "filter"));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("false", "0", false)]
    [InlineData("true", "0", true)]
    [InlineData("false", "1", true)]
    [InlineData("false", "true", true)]
    public void FilterReset_OnlyClearsSelectedFilterOptions(string selected, string value, bool clear)
    {
        string source = $"""
            <App>
              <Button name="Unrelated" selected="true" />
              <Button name="NativeFlightResultsFilterOption-AIRLINE_NAME-KLM" selected="{selected}" value="{value}" />
              <Button name="NativeFlightResultsClearFilters" enabled="false" />
            </App>
            """;
        Assert.Equal(clear ? NativeSearchAccessibilityIds.ClearFilters : NativeSearchAccessibilityIds.ApplyFilters,
            NativeFlightSearchPage.FilterResetAction(source));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("London to New York", "LHR", "JFK")]
    [InlineData("New York to London", "JFK", "LHR")]
    [InlineData("Manchester to New York", "MAN", "JFK")]
    [InlineData("LHR → JFK", "LHR", "JFK")]
    [InlineData("London to Lagos", "LHR", "LOS")]
    [InlineData("Lagos to London", "LOS", "LHR")]
    [InlineData("LHR → LOS", "LHR", "LOS")]
    public void SummaryRoute_PreservesDisplayedEndpointOrder(string text, string origin, string destination)
        => Assert.Equal((origin, destination), NativeFlightSearchPage.ReadOrderedSummaryRoute(text));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("London Heathrow New York")]
    [InlineData("Paris to Amsterdam")]
    public void SummaryRoute_MissingOrUnsupportedOrderedHeaderFails(string text)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.ReadOrderedSummaryRoute(text));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("London to New York", "Economy Classic", true)]
    [InlineData("New York to London", "Economy Classic", false)]
    [InlineData("London to New York", "", false)]
    [InlineData("London to New York", "Premium", false)]
    public void SummaryAssertions_RejectReversedRouteAndMissingOrWrongFare(string route, string observedFare, bool valid)
    {
        (string origin, string destination) = NativeFlightSearchPage.ReadOrderedSummaryRoute(route);
        FlightSummary summary = new(origin, destination, observedFare, route, "£500", true);
        Action assertion = () => NativeFlightSearchPage.AssertSummaryRouteAndFare(summary, "LHR", "JFK", "Economy Classic");
        if (valid)
        {
            assertion();
        }
        else
        {
            Assert.Throws<InvalidOperationException>(assertion);
        }
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("VS", "Virgin Atlantic", "Virgin Atlantic")]
    [InlineData("KL", "KLM", "KLM")]
    [InlineData("DL", "Delta Air Lines +1", "Delta Air Lines")]
    public void CarrierEvidence_UsesScopedCarrierRowNotAggregateCard(string code, string name, string expectedName)
    {
        string source = $"""
            <App>
              <Other name="NativeFlightResult0" label="Whole card including Virgin Atlantic and fares">
                <Other name="NativeFlightResult0Carrier{code}" label="Dates and times, not the airline name">
                  <Text type="XCUIElementTypeStaticText" label="Departure time is 09:00" />
                  <Text type="XCUIElementTypeStaticText" label="Departure airport LHR" />
                  <Other><Text type="XCUIElementTypeStaticText" label="{name}" /></Other>
                  <Text type="XCUIElementTypeStaticText" label="Arrival airport JFK" />
                </Other>
              </Other>
              <Other name="NativeFlightResult1CarrierVS">
                <Text type="XCUIElementTypeStaticText" label="Departure airport LHR" />
                <Text type="XCUIElementTypeStaticText" label="Virgin Atlantic" />
                <Text type="XCUIElementTypeStaticText" label="Arrival airport JFK" />
              </Other>
            </App>
            """;
        Assert.Equal((code, expectedName), NativeFlightSearchPage.ReadOperatingCarrier(source, "NativeFlightResult0"));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("<App><Other name='NativeFlightResult0' label='KLM' /></App>")]
    [InlineData("<App><Other name='NativeFlightResult1CarrierKL' label='KLM' /></App>")]
    [InlineData("<App><Other name='NativeFlightResult0CarrierKL' label='whole card' /></App>")]
    public void CarrierEvidence_MissingCarrierNameDoesNotFallBackToCardOrAnotherFlight(string source)
        => Assert.Throws<InvalidOperationException>(() => NativeFlightSearchPage.ReadOperatingCarrier(source, "NativeFlightResult0"));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void FareBaggage_UpdatedContractIsRunnableAndReady()
    {
        MethodInfo method = typeof(FareBenefitsAndBaggageTests).GetMethod(
            nameof(FareBenefitsAndBaggageTests.SearchBook_TC29_SelectedFareBenefitsAndBaggage_PropagateToFlightSummary))
            ?? throw new InvalidOperationException("Fare/baggage scenario was not found.");
        Assert.Null(Assert.IsType<FactAttribute>(method.GetCustomAttribute<FactAttribute>()).Skip);
        Assert.Contains(method.CustomAttributes, attribute => attribute.AttributeType == typeof(TraitAttribute) &&
            Equals(attribute.ConstructorArguments[0].Value, "Readiness") &&
            Equals(attribute.ConstructorArguments[1].Value, "Ready"));
        JObject contract = JObject.Parse(File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "Contracts", "search-book-automation-contract.json")));
        JToken scenario = Assert.Single(Assert.IsType<JArray>(contract["scenarios"]), item => item.Value<string>("id") == "TC29");
        Assert.Equal("Ready", scenario.Value<string>("readiness"));
    }
}
