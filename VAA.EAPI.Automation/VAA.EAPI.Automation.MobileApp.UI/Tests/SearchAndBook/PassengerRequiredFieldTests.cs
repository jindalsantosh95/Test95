using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class PassengerRequiredFieldTests : SearchResultsJourneyCoverageTestBase
{
    public PassengerRequiredFieldTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Passenger-Validation")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC34_MissingPassengerFirstName_ShowsErrorAndPreventsSave()
    {
        const string testName = nameof(SearchBook_TC34_MissingPassengerFirstName_ShowsErrorAndPreventsSave);
        try
        {
            NativeFlightSearchPage search = OpenOneWayResults(FlightSearch.OneWay("LHR", "JFK", 3), testName);
            search.SelectFirstFlightByCarrier(virginFlight: true, expectFlightSummary: true);
            NativeBookingPage booking = new(Fixture.Driver);
            booking.ContinueFromSummary();
            booking.VerifyMissingFirstNameBlocksTravellerSave();
            TrySetBrowserStackStatus("passed", "Required first-name error retained the passenger editor without saving.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
