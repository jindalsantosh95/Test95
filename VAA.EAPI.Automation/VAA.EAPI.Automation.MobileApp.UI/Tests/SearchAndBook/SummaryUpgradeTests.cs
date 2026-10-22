using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SummaryUpgradeTests(ITestOutputHelper output) : ResultsRefinementTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Flight-Summary")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC39_OneWaySummaryUpgrade_UpdatesFareAndTotalPreservingFlight()
        => RunRefinementTest(page =>
        {
            FlightSearch search = FlightSearch.OneWay("LHR", "JFK", monthsFromToday: 3, PassengerCounts.OneAdult);
            SelectedFlight original = page.SelectFirstFlightByCarrier(true, expectFlightSummary: true, requiredCabin: "Economy");
            page.AssertFlightSummaryMatchesSelection(search, original, expectVirginOperatingCarrier: true);
            string originalFare = page.ReadSummaryFareTitle();
            string originalPrice = page.ReadSummary().PriceText;
            SummaryUpgradeOffer offer = page.ReadOfferedSummaryUpgrade();
            _ = SummaryUpgradeAssertions.ExpectedTotal(originalPrice, offer);

            page.ApplySummaryUpgrade(offer);

            page.AssertFlightSummaryMatchesSelection(search, original with { CabinName = offer.FareTitle },
                expectVirginOperatingCarrier: true);
            SummaryUpgradeAssertions.MatchesOffer(originalFare, originalPrice, offer,
                page.ReadSummaryFareTitle(), page.ReadSummary().PriceText);
        }, "Summary upgrade retained the flight and displayed the offered fare and recalculated currency total.");
}
