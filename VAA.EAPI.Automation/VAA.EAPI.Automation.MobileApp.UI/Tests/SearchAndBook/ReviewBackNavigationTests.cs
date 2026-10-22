using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Handlers;
using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class ReviewBackNavigationTests(ITestOutputHelper output) : SearchBookTestBase(output,
    new Dictionary<string, string?>
    {
        ["IOSLaunch:AutomationFeatureFlags"] = "850100-search-native-entry,850101-search-native-skip-seat-selection",
    })
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Readiness")]
    [Trait("Area", "SearchAndBook")]
    [Trait("Journey", "ReviewTotal")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC56_GuestReviewBackNavigation_ReentryRetainsTotalAndPaymentReadiness()
    {
        const string testName = nameof(SearchBook_TC56_GuestReviewBackNavigation_ReentryRetainsTotalAndPaymentReadiness);
        TrySetBrowserStackSessionName(testName);
        try
        {
            new AppEntryHandler(Fixture.Driver).EnterAsGuest();
            NativeFlightSearchPage search = new(Fixture.Driver);
            search.Open();
            search.SearchOneWay(FlightSearch.OneWay("LHR", "JFK", 3, PassengerCounts.OneAdult));
            search.WaitForResults();
            search.SelectFirstFlightByCarrier(virginFlight: true, expectFlightSummary: true,
                requiredCabin: "Economy", requiredFareTitle: "Economy Classic");
            new NativeBookingCheckoutFlow(Fixture.Driver).VerifyGuestReviewBackNavigation();
            TrySetBrowserStackStatus("passed", "Review back/re-entry retained total and readiness; no HPP or payment.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
