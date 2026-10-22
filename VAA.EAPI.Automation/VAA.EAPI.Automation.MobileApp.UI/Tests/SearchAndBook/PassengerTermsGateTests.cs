using VAA.EAPI.Automation.MobileApp.UI.Flows;
using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class PassengerTermsGateTests : SearchResultsJourneyCoverageTestBase
{
    public PassengerTermsGateTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Feature", "Passenger-Terms")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC35_ValidGuestPassenger_TermsAcceptanceControlsContinue()
    {
        const string testName = nameof(SearchBook_TC35_ValidGuestPassenger_TermsAcceptanceControlsContinue);
        try
        {
            NativeFlightSearchPage search = OpenOneWayResults(FlightSearch.OneWay("LHR", "JFK", 3), testName);
            search.SelectFirstFlightByCarrier(virginFlight: true, expectFlightSummary: true);
            new NativeBookingCheckoutFlow(Fixture.Driver)
                .PrepareGuestPassengerDetailsWithoutAcceptingTerms(PassengerCounts.OneAdult);
            NativeBookingPage booking = new(Fixture.Driver);
            booking.SetBookingTermsAccepted(false);
            Assert.False(booking.WaitForPassengerContinueState(enabled: false));
            booking.SetBookingTermsAccepted(true);
            Assert.True(booking.WaitForPassengerContinueState(enabled: true));
            booking.SetBookingTermsAccepted(false);
            Assert.False(booking.WaitForPassengerContinueState(enabled: false));
            TrySetBrowserStackStatus("passed", "Valid traveller: Terms alone enabled then disabled Continue; no payment attempted.", testName);
        }
        catch (Exception exception)
        {
            TrySetBrowserStackStatus("failed", DescribeBrowserStackFailure(exception), testName);
            throw;
        }
    }
}
