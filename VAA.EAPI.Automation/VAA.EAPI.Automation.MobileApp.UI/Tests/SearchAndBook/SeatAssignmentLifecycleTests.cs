using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SeatAssignmentLifecycleTests(ITestOutputHelper output) : SeatScenarioTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC44_OneAdultSeatSelection_ReplacementAndRemovalUpdateAssignment()
        => RunSeatScenario(nameof(SearchBook_TC44_OneAdultSeatSelection_ReplacementAndRemovalUpdateAssignment), seats =>
        {
            string initial = seats.ReadPassengerAssignment();
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(initial), "Expected an unassigned traveller.");
            string first = seats.OpenAvailableSeat();
            seats.ConfirmSeat(first);
            string replacement = seats.OpenAvailableSeat(excludingSeat: first);
            Assert.NotEqual(first, replacement);
            seats.ConfirmSeat(replacement);
            Assert.False(NativeBookingPage.PassengerHasSeat(seats.ReadPassengerAssignment(), first),
                "The original seat remained assigned after replacement.");
            seats.RemoveSeat(replacement, initial);
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(seats.ReadPassengerAssignment()));
        });
}
