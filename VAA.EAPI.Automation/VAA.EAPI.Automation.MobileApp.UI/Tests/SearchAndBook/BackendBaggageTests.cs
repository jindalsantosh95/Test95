using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using VAA.EAPI.Automation.MobileApp.UI.Requests;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class BackendBaggageTests : SearchResultsJourneyCoverageTestBase
{
    public BackendBaggageTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-BackendValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "BackendRegression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Search-BaggageBackendParity")]
    [Trait("Readiness", "Ready")]
    [Trait("Platform", "iOS")]
    public async Task SearchBook_TC41_OneWaySummaryBaggage_MatchesBackendSelectedFlightAndFare()
    {
        const string testName = nameof(SearchBook_TC41_OneWaySummaryBaggage_MatchesBackendSelectedFlightAndFare);
        try
        {
            using BackendSearchClient backend = new(Fixture.Config);
            FlightSearch search = FlightSearch.OneWay("LHR", "JFK", monthsFromToday: 3);
            BackendFareExpectation expected = await backend.PrepareAsync(search);
            NativeFlightSearchPage page = OpenOneWayResults(search, testName);
            IReadOnlyList<string> baggage = page.SelectBackendMatchedFareAndReadBaggage(expected);
            Output.WriteLine(BackendBaggageAssertions.AssertMatches(expected.Baggage, baggage));
            TrySetBrowserStackStatus("passed", "Summary baggage matched the independently prepared backend flight and fare, including dimensions.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
