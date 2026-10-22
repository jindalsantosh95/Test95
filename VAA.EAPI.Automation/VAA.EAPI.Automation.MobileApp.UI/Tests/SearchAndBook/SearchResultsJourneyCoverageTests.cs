using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public abstract class SearchResultsJourneyCoverageTestBase : SearchBookTestBase
{
    protected const string ControlledSearchFailureReason =
        "Draft until the iOS test build exposes a deterministic fail-once Search and Book service hook.";

    protected SearchResultsJourneyCoverageTestBase(ITestOutputHelper output) : base(output) { }

    protected NativeFlightSearchPage OpenOneWayResults(
        FlightSearch search,
        string testName)
    {
        TrySetBrowserStackSessionName(testName);
        NativeFlightSearchPage page = new(Fixture.Driver);
        new AppEntryHandler(Fixture.Driver).EnterAsGuest();
        page.Open();
        page.SearchOneWay(search);
        page.WaitForResults();
        return page;
    }

    protected NativeFlightSearchPage OpenReturnInboundResults(
        FlightSearch search,
        string testName)
    {
        TrySetBrowserStackSessionName(testName);
        NativeFlightSearchPage page = new(Fixture.Driver);
        new AppEntryHandler(Fixture.Driver).EnterAsGuest();
        page.Open();
        page.SearchReturn(search);
        page.WaitForResults();
        page.SelectFirstFlightByCarrier(virginFlight: true, expectFlightSummary: false);
        return page;
    }
}

public sealed class FlexibleDateSearchTests : SearchResultsJourneyCoverageTestBase
{
    public FlexibleDateSearchTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-FlexibleDates")]
    [Trait("Readiness", "Ready")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC27_FlexibleDateSelection_RefreshesResultsAndPreservesRoute()
    {
        const string testName = nameof(SearchBook_TC27_FlexibleDateSelection_RefreshesResultsAndPreservesRoute);
        try
        {
            FlightSearch search = FlightSearch.OneWay("LHR", "JFK", monthsFromToday: 3);
            NativeFlightSearchPage page = OpenOneWayResults(search, testName);
            DateOnly selectedDate = page.SelectAnotherFlexibleDateAndAssertResultsRefresh(
                search.Origin,
                search.Destination);
            TrySetBrowserStackStatus("passed",
                $"Flexible date {selectedDate:yyyy-MM-dd} completed, remained selected in the refreshed results, " +
                "and preserved the route.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}

public sealed class EditSubmittedSearchTests : SearchResultsJourneyCoverageTestBase
{
    public EditSubmittedSearchTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-EditSearch")]
    [Trait("Suite", "Sanity")]
    [Trait("Readiness", "Ready")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC28_EditSubmittedSearch_UpdatesCriteriaAndResultsRoute()
    {
        const string testName = nameof(SearchBook_TC28_EditSubmittedSearch_UpdatesCriteriaAndResultsRoute);
        try
        {
            FlightSearch initial = FlightSearch.Return("LHR", "JFK", monthsFromToday: 3);
            NativeFlightSearchPage page = OpenReturnInboundResults(initial, testName);
            // Edit is reached from inbound results, whose displayed direction is reversed.
            page.AssertResultsRouteContains(initial.Destination, initial.Origin);

            page.EditSearch();
            FlightSearch amended = FlightSearch.Return("MAN", "JFK", monthsFromToday: 4);
            page.SearchReturn(amended);
            page.WaitForResults();
            page.AssertResultsRouteContains(amended.Origin, amended.Destination);
            page.AssertResultsSelectedDate(amended.DepartureDate);

            TrySetBrowserStackStatus("passed",
                $"Edit search changed the submitted route from LHR-JFK to MAN-JFK and displayed results for " +
                $"{amended.DepartureDate:yyyy-MM-dd}.",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}

public sealed class FareBenefitsAndBaggageTests : SearchResultsJourneyCoverageTestBase
{
    public FareBenefitsAndBaggageTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-FareBenefitsBaggage")]
    [Trait("Readiness", "Ready")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC29_SelectedFareBenefitsAndBaggage_PropagateToFlightSummary()
    {
        const string testName = nameof(SearchBook_TC29_SelectedFareBenefitsAndBaggage_PropagateToFlightSummary);
        try
        {
            FlightSearch search = FlightSearch.OneWay("LHR", "JFK", monthsFromToday: 3);
            NativeFlightSearchPage page = OpenOneWayResults(search, testName);
            FareBaggageObservation observation = page.SelectFirstFareAndReadBenefitsAndSummaryBaggage();

            string coverage = FareBaggageAssertions.AssertMatches(observation);
            Output.WriteLine(coverage);

            TrySetBrowserStackStatus("passed",
                $"Selected fare '{observation.FareLabel}' exposed {observation.BenefitLabels.Count} benefit(s) " +
                $"and Flight Summary exposed {observation.BaggageAllowances.Count} baggage allowance(s).",
                testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}

public sealed class ControlledSearchRecoveryTests : SearchResultsJourneyCoverageTestBase
{
    public ControlledSearchRecoveryTests(ITestOutputHelper output) : base(output) { }

    [Fact(Skip = ControlledSearchFailureReason)]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Deferred")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Feature", "Search-ErrorRecovery")]
    [Trait("Readiness", "Draft")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC30_TransientSearchFailure_RetryRecoversToResults()
        => throw new InvalidOperationException(ControlledSearchFailureReason);
}
