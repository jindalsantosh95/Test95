using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class SummaryChangeAssertionsTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Select +£0", 500.0)]
    [InlineData("Select +£25.50", 525.5)]
    [InlineData("Select −£25", 475.0)]
    [InlineData("Select -£25", 475.0)]
    [InlineData("Select £650", 650.0)]
    public void ReturnTotal_DistinguishesInboundAdjustmentFromFullTotal(string inbound, double expected)
        => Assert.Equal(("£", (decimal)expected), SummaryChangeAssertions.ReturnTotal("£500", inbound));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Select")]
    [InlineData("+$25")]
    [InlineData("+£25 +£30")]
    [InlineData("-£500")]
    [InlineData("£0")]
    [InlineData("£12.345")]
    public void ReturnTotal_RejectsMissingAmbiguousOrInvalidAmounts(string inbound)
        => Assert.Throws<InvalidOperationException>(() => SummaryChangeAssertions.ReturnTotal("£500", inbound));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Select £1,234.50", "£", 1234.50)]
    [InlineData("$500 Total $500", "$", 500.0)]
    public void Money_ReadsOneDistinctCurrencyAmount(string text, string currency, double amount)
        => Assert.Equal((currency, (decimal)amount), SummaryChangeAssertions.Money(text));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("£0")]
    [InlineData("£500 £600")]
    [InlineData("£500 $500")]
    [InlineData("£12.345")]
    [InlineData("£1,23")]
    public void Money_RejectsMissingZeroOrAmbiguousEvidence(string text)
        => Assert.Throws<InvalidOperationException>(() => SummaryChangeAssertions.Money(text));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("", "Economy Classic", "£600", false)]
    [InlineData("09:00 12:00", "Economy Classic", "£600", false)]
    [InlineData("10:00 13:00", "Economy Light", "£600", false)]
    [InlineData("10:00 13:00", "Economy Classic", "£500", false)]
    [InlineData("10:00 13:00", "Economy Classic", "$600", false)]
    [InlineData("10:00 13:00", "Economy Classic", "£600", true)]
    public void Replacement_RejectsSameScheduleWrongFareOrStalePrice(
        string schedule, string fare, string price, bool valid)
    {
        SelectedFlight original = new(0, "Economy", "09:00 12:00", "VS", "Virgin Atlantic");
        SelectedFlight replacement = new(1, "Economy", schedule, "VS", "Virgin Atlantic")
        { FareTitle = "Economy Classic", FareLabel = "Select Economy Classic £600" };
        Exception? failure = Record.Exception(() => SummaryChangeAssertions.MatchesReplacement(
            original, replacement, fare, price));
        if (valid)
        {
            Assert.Null(failure);
        }
        else
        {
            Assert.NotNull(failure);
        }
    }
}
