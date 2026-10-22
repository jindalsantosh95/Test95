using VAA.EAPI.Automation.MobileApp.UI.Models;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class TwoPassengerSeatAssignmentTests(ITestOutputHelper output) : SeatScenarioTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC46_TwoAdultSeatSelection_RetainsIndependentAssignments()
        => RunSeatScenario(nameof(SearchBook_TC46_TwoAdultSeatSelection_RetainsIndependentAssignments), seats =>
        {
            seats.WaitForActivePassenger(0);
            string firstInitial = seats.ReadPassengerAssignment(0);
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(firstInitial));
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(seats.ReadPassengerAssignment(1)));

            string firstSeat = seats.OpenAvailableSeat();
            seats.ConfirmSeat(firstSeat, passengerIndex: 0);
            seats.WaitForActivePassenger(1);
            seats.WaitForSeatUnavailable(firstSeat);
            string firstAssigned = seats.ReadPassengerAssignment(0);

            string secondSeat = seats.OpenAvailableSeat(excludingSeat: firstSeat);
            Assert.NotEqual(firstSeat, secondSeat);
            seats.ConfirmSeat(secondSeat, passengerIndex: 1);
            string secondAssigned = seats.ReadPassengerAssignment(1);

            seats.SelectPassenger(0);
            seats.WaitForAssignment(firstAssigned, passengerIndex: 0);
            seats.WaitForSeatUnavailable(secondSeat);
            seats.SelectPassenger(1);
            seats.WaitForAssignment(secondAssigned, passengerIndex: 1);
            seats.WaitForSeatUnavailable(firstSeat);

            seats.SelectPassenger(0);
            seats.RemoveSeat(firstSeat, firstInitial, passengerIndex: 0);
            seats.WaitForAssignment(secondAssigned, passengerIndex: 1);
            seats.SelectPassenger(1);
            seats.WaitForAssignment(secondAssigned, passengerIndex: 1);
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(seats.ReadPassengerAssignment(0)));
        }, passengers: new PassengerCounts(Adults: 2));
}
