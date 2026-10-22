using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SummaryChangeTests(ITestOutputHelper output) : ResultsRefinementTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Flight-Summary")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC38_OneWaySummaryChange_RetainsReplacementFlightFareAndPrice()
        => RunRefinementTest(page =>
        {
            FlightSearch search = FlightSearch.OneWay("LHR", "JFK", monthsFromToday: 3, PassengerCounts.OneAdult);
            SelectedFlight original = page.SelectFirstFlightByCarrier(true, expectFlightSummary: true);
            page.AssertFlightSummaryMatchesSelection(search, original, expectVirginOperatingCarrier: true);

            page.ChangeSummaryFlight();
            SelectedFlight replacement = page.SelectFirstFlightByCarrier(true, expectFlightSummary: true,
                excludedFlight: original);

            page.AssertFlightSummaryMatchesSelection(search, replacement, expectVirginOperatingCarrier: true);
            SummaryChangeAssertions.MatchesReplacement(original, replacement,
                page.ReadSummaryFareTitle(), page.ReadSummary().PriceText);
        }, "Summary Change retained the replacement schedule, fare title and single-adult currency price.");
}
