using System.Runtime.CompilerServices;
using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class FlightSummaryTestBase : ResultsRefinementTestBase
{
    protected FlightSummaryTestBase(ITestOutputHelper output) : base(output) { }

    protected void RunFlightSummaryValidation(
        bool virginOperatingCarrier,
        string successReason,
        [CallerMemberName] string testName = "")
        => RunRefinementTest(
            page =>
            {
                FlightSearch search = FlightSearch.OneWay(
                    "LHR", "JFK", monthsFromToday: 3, PassengerCounts.OneAdult);
                SelectedFlight selected = page.SelectFirstFlightByCarrier(
                    virginOperatingCarrier,
                    expectFlightSummary: true);
                page.AssertFlightSummaryMatchesSelection(search, selected, virginOperatingCarrier);
            },
            successReason,
            testName);
}

public sealed class VirginFlightSummaryTests : FlightSummaryTestBase
{
    public VirginFlightSummaryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Flight-Summary")]
    [Trait("Carrier", "Virgin")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC24_VirginOperatingCarrier_FlightSummaryMatchesSelectedFlight()
        => RunFlightSummaryValidation(
            virginOperatingCarrier: true,
            "The Flight Summary retained the selected Virgin flight, fare, schedule and price.");
}

public sealed class OtherOperatingCarrierFlightSummaryTests : FlightSummaryTestBase
{
    public OtherOperatingCarrierFlightSummaryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Flight-Summary")]
    [Trait("Carrier", "Partner")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC25_OtherOperatingCarrier_FlightSummaryMatchesSelectedFlight()
        => RunFlightSummaryValidation(
            virginOperatingCarrier: false,
            "The Flight Summary retained the selected partner flight, backend fare, schedule and price.");
}

public sealed class MultiCarrierConnectingFlightSummaryTests : FlightSummaryTestBase
{
    public MultiCarrierConnectingFlightSummaryTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Flight-Summary")]
    [Trait("Carrier", "MultiCarrier")]
    [Trait("Journey", "Connecting")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC26_MultiCarrierConnectingFlight_FlightSummaryRetainsStopsAndAdditionalCarrier()
        => RunRefinementTest(
            page =>
            {
                FlightSearch search = FlightSearch.OneWay(
                    "LHR", "JFK", monthsFromToday: 3, PassengerCounts.OneAdult);
                SelectedFlight selected = page.SelectFirstMultiCarrierConnectingFlight();
                page.AssertMultiCarrierConnectionIsRetainedOnSummary(search, selected);
                ConnectingFlightDetails resultDetails = selected.Details
                    ?? throw new InvalidOperationException("The selected result has no observed per-leg details.");
                ConnectingFlightAssertions.MatchesSearch(search, resultDetails);
                ConnectingFlightDetails summaryDetails = page.ReadSummaryConnectingFlightDetails(resultDetails.Legs.Count - 1);
                ConnectingFlightAssertions.MatchesSearch(search, summaryDetails);
                ConnectingFlightAssertions.Retained(resultDetails, summaryDetails);
            },
            "Flight Summary retained every connecting leg's airports, dates, times and operating carrier, plus stop count and +1.");
}
