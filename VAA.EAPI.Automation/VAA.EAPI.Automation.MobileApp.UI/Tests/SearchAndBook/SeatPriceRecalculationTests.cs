using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.SearchAndBook;

public sealed class SeatPriceRecalculationTests(ITestOutputHelper output) : SeatScenarioTestBase(output)
{
    [Fact]
    [Trait("Category", "MobileApp-SearchBook-FunctionalValidation")]
    [Trait("TestType", "FunctionalValidation")]
    [Trait("Suite", "Regression")]
    [Trait("Suite", "Sanity")]
    [Trait("RegressionArea", "SearchAndBook")]
    [Trait("Journey", "Seats")]
    [Trait("Platform", "iOS")]
    public void SearchBook_TC47_PaidSeatReplacementAndRemoval_RecalculateChargeAndTripTotal()
        => RunSeatScenario(nameof(SearchBook_TC47_PaidSeatReplacementAndRemoval_RecalculateChargeAndTripTotal), seats =>
        {
            seats.WaitForActivePassenger(0);
            string unassigned = seats.ReadPassengerAssignment();
            Assert.True(NativeSeatSelectionPage.IsUnassignedPassenger(unassigned));
            Assert.Equal(0m, seats.ReadSeatCharge());
            decimal baseline = seats.ReadTripTotal();
            Assert.True(baseline > 0, "Original trip total must be positive.");
            (string first, decimal firstPrice) = seats.OpenPaidSeat();
            seats.ConfirmSeat(first);
            seats.WaitForPrices(firstPrice, baseline + firstPrice);
            (string replacement, decimal replacementPrice) = seats.OpenPaidSeat(differentFromPrice: firstPrice);
            Assert.NotEqual(first, replacement);
            Assert.NotEqual(firstPrice, replacementPrice);
            seats.ConfirmSeat(replacement);
            seats.WaitForPrices(replacementPrice, baseline + replacementPrice);
            seats.RemoveSeat(replacement, unassigned);
            seats.WaitForPrices(0m, baseline);
        });
}
