using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SeatSheetCancellationTests(ITestOutputHelper output) : SeatScenarioTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC45_SeatDetailsAndKey_DismissalPreservesPassengerAssignment()
        => RunSeatScenario(nameof(SearchBook_TC45_SeatDetailsAndKey_DismissalPreservesPassengerAssignment), seats =>
        {
            string initial = seats.ReadPassengerAssignment();
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(initial));
            string offered = seats.OpenAvailableSeat();
            seats.CloseDetails(initial);
            seats.OpenAndCloseKey(initial);
            seats.OpenSeat(offered);
            seats.ConfirmSeat(offered);
            string assigned = seats.ReadPassengerAssignment();
            seats.OpenAvailableSeat(excludingSeat: offered);
            seats.CloseDetails(assigned);
            seats.OpenAndCloseKey(assigned);
            Assert.True(NativeBookingPage.PassengerHasSeat(seats.ReadPassengerAssignment(), offered));
        });
}
