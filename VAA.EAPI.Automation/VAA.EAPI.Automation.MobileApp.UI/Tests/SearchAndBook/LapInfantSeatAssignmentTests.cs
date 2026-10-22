using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class LapInfantSeatAssignmentTests(ITestOutputHelper output) : SeatScenarioTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC49_AdultWithLapInfant_OnlyAdultHasIndependentSeatAssignment()
        => RunSeatScenario(nameof(SearchBook_TC49_AdultWithLapInfant_OnlyAdultHasIndependentSeatAssignment), seats =>
        {
            Assert.Equal(1, seats.ReadAssignablePassengerCount());
            string initial = seats.ReadPassengerAssignment();
            Assert.Contains("+ infant", initial, StringComparison.Ordinal);
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(initial));
            string first = seats.OpenAvailableSeat();
            seats.ConfirmSeat(first);
            seats.WaitForActivePassenger(0);
            Assert.Equal(1, seats.ReadAssignablePassengerCount());
            string replacement = seats.OpenAvailableSeat(excludingSeat: first);
            Assert.NotEqual(first, replacement);
            seats.ConfirmSeat(replacement);
            seats.WaitForActivePassenger(0);
            Assert.False(NativeBookingPage.PassengerHasSeat(seats.ReadPassengerAssignment(), first));
            seats.RemoveSeat(replacement, initial);
            seats.WaitForActivePassenger(0);
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(seats.ReadPassengerAssignment()));
            Assert.Equal(1, seats.ReadAssignablePassengerCount());
        }, new PassengerCounts(Adults: 1, Infants: 1));
}
