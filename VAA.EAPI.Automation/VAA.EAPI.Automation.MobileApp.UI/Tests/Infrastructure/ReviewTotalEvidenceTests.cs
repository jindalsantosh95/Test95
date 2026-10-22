using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class ReviewTotalEvidenceTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("£100.00", "£20.00", "£120.00")]
    [InlineData("$100.00", "$20.00", "$120.00")]
    [InlineData("33,000 points", "£20.00", "33,000 points + £20.00")]
    public void RetainedFigures_AcceptsSameAmountsRegardlessOfRowOrder(string flights, string taxes, string total)
        => ReviewTotalAssertions.VerifyRetainedFigures(
            [$"Flights, {flights}", $"Taxes, fees and charges, {taxes}", $"Total to pay, {total}"],
            [$"Total to pay, {total}", $"Taxes, fees and charges, {taxes}", $"Flights, {flights}"]);

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("£101.00", "£19.00", "£120.00")]
    [InlineData("$100.00", "$20.00", "$120.00")]
    [InlineData("100 points", "£20.00", "100 points + £20.00")]
    [InlineData("£100.00", "£20.00", "£121.00")]
    public void RetainedFigures_RejectsChangedBreakdownCurrencyPointsOrTotal(string flights, string taxes, string total)
        => Assert.ThrowsAny<Exception>(() => ReviewTotalAssertions.VerifyRetainedFigures(
            ["Flights, £100.00", "Taxes, fees and charges, £20.00", "Total to pay, £120.00"],
            [$"Flights, {flights}", $"Taxes, fees and charges, {taxes}", $"Total to pay, {total}"]));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Seat selection, £11.00")]
    [InlineData("Seat selection, $10.00")]
    [InlineData("Unrelated row")]
    public void RetainedFigures_RejectsChangedOrMissingSeatCharge(string seatRow)
        => Assert.ThrowsAny<Exception>(() => ReviewTotalAssertions.VerifyRetainedFigures(
            ["Flights, £100.00", "Taxes, fees and charges, £20.00", "Seat selection, £10.00", "Total to pay, £130.00"],
            ["Flights, £100.00", "Taxes, fees and charges, £20.00", seatRow, "Total to pay, £130.00"]));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SeatDetail_RecognizesExplicitFreeLabelOnly()
    {
        Assert.Equal(0m, Models.SeatPriceEvidence.ReadDetailGbp("Seat price: Free"));
        Assert.Equal(10m, Models.SeatPriceEvidence.ReadDetailGbp("Seat price: £10.00"));
        Assert.Throws<InvalidOperationException>(() => Models.SeatPriceEvidence.ReadDetailGbp("Seat price:"));
        Assert.Throws<InvalidOperationException>(() => Models.SeatPriceEvidence.ReadDetailGbp("Seat price: unavailable"));
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(", ")]
    [InlineData("\n")]
    public void DesignCopy_AcceptsNativeCombinedLabelSeparators(string separator)
        => ReviewTotalAssertions.VerifyCopy("Fare conditions and details",
            "Enable international payments" + separator +
            "To ensure a smooth payment, check if your card is enabled for international or online payments.",
            "Continue to secure payment");

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Wrong link", "Enable international payments, To ensure a smooth payment, check if your card is enabled for international or online payments.", "Continue to secure payment")]
    [InlineData("Fare conditions and details", "Enable international payments", "Continue to secure payment")]
    [InlineData("Fare conditions and details", "Enable international payments, To ensure a smooth payment, check if your card is enabled for international or online payments.", "Pay now")]
    public void DesignCopy_RejectsChangedOrIncompleteCopy(string link, string guidance, string button)
        => Assert.ThrowsAny<Exception>(() => ReviewTotalAssertions.VerifyCopy(link, guidance, button));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("£")]
    [InlineData("$")]
    [InlineData("€")]
    public void RevenueBreakdown_RetainsSummaryCurrencyAndTotal(string currency)
        => ReviewTotalAssertions.Verify([
            $"Flights, {currency}987.00", $"Taxes, fees and charges, {currency}342.39",
            $"Total to pay, Includes taxes, {currency}1,329.39"], $"1 person, {currency}1,329.39", false);

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void RewardBreakdown_RetainsPointsAndCashTaxes()
        => ReviewTotalAssertions.Verify(["Flights, 33,000 points", "Taxes, fees and charges, £254.20",
            "Total to pay, 33,000 points + £254.20"], "1 person, 33,000 points + £254.20", false);

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void SeatBreakdown_AddsDisplayedChargeToPreSeatSummary()
        => ReviewTotalAssertions.Verify(["Flights, £100.00", "Taxes, fees and charges, £20.00",
            "Seat selection, £10.00", "Total to pay, £130.00"], "£120.00", true, 10m);

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(null)]
    [InlineData(9)]
    public void SeatBreakdown_RejectsMissingOrDifferentSelectionEvidence(int? selectedCharge)
        => Assert.ThrowsAny<Exception>(() => ReviewTotalAssertions.Verify([
            "Flights, £100.00", "Taxes, fees and charges, £20.00",
            "Seat selection, £10.00", "Total to pay, £130.00"], "£120.00", true, selectedCharge));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void FreeSelectedSeat_DoesNotRequireChargeRow()
        => ReviewTotalAssertions.Verify(["Flights, £100.00", "Taxes, fees and charges, £20.00",
            "Total to pay, £120.00"], "£120.00", true, 0m);

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void PaidSelectedSeat_RejectsMissingChargeRow()
        => Assert.ThrowsAny<Exception>(() => ReviewTotalAssertions.Verify([
            "Flights, £100.00", "Taxes, fees and charges, £20.00", "Total to pay, £120.00"],
            "£120.00", true, 10m));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("£120.00", "Flights, £101.00", "Taxes, fees and charges, £20.00", "Total to pay, £120.00")]
    [InlineData("£120.00", "Flights, £1,00", "Taxes, fees and charges, £20.00", "Total to pay, £120.00")]
    [InlineData("£120.00", "Flights, £100.00 £100.00", "Taxes, fees and charges, £20.00", "Total to pay, £120.00")]
    [InlineData("$120.00", "Flights, £100.00", "Taxes, fees and charges, £20.00", "Total to pay, £120.00")]
    [InlineData("£121.00", "Flights, £100.00", "Taxes, fees and charges, £20.00", "Total to pay, £120.00")]
    [InlineData("£120.00", "Flights, £100.00", "Wrong label, £20.00", "Total to pay, £120.00")]
    [InlineData("33,000 points + £20.00", "Flights, 32,000 points", "Taxes, fees and charges, £20.00", "Total to pay, 32,000 points + £20.00")]
    public void IncorrectBreakdown_Fails(string baseline, string flights, string taxes, string total)
        => Assert.ThrowsAny<Exception>(() => ReviewTotalAssertions.Verify([flights, taxes, total], baseline, false));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void UnexpectedGuestSeatCharge_Fails()
        => Assert.ThrowsAny<Exception>(() => ReviewTotalAssertions.Verify([
            "Flights, £100.00", "Taxes, fees and charges, £20.00", "Seat selection, £10.00",
            "Total to pay, £130.00"], "£120.00", false));

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void PriceReader_IgnoresUnrelatedAndInaccessibleText()
    {
        string source = """
            <App><Text accessible="true" label="Flights, £999" />
              <Other name="NativeFlightPaymentSummaryCard" visible="true">
                <Other accessible="true" label="Flights, £100.00" />
                <Text accessible="false" label="£100.00" />
                <Other accessible="true" label="Taxes, fees and charges, £20.00" />
                <Other accessible="true" label="Total to pay, £120.00" />
              </Other>
            </App>
            """;
        ReviewTotalAssertions.Verify(NativeBookingPage.ReadPaymentSummaryRows(source), "£120.00", false);
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void PriceReader_RequiresOwnedContainer()
        => Assert.Throws<InvalidOperationException>(() => NativeBookingPage.ReadPaymentSummaryRows("<App />"));
}
