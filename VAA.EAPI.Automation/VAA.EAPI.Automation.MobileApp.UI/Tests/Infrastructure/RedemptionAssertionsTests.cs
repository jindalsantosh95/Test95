using VAA.EAPI.Automation.MobileApp.UI.Assertions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class RedemptionAssertionsTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("100,000", 100000)]
    [InlineData("100000 Virgin Points", 100000)]
    [InlineData("Virgin Points: 250,000", 250000)]
    public void Balance_ReadsObservedWholePoints(string text, int expected)
        => Assert.Equal(expected, RedemptionAssertions.ReadBalance(text));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("-100")]
    [InlineData("100.50")]
    [InlineData("100,000 / 200,000")]
    public void Balance_RejectsMissingOrAmbiguousEvidence(string text)
        => Assert.Throws<InvalidOperationException>(() => RedemptionAssertions.ReadBalance(text));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(10000, 100000, true)]
    [InlineData(10000, 10000, true)]
    [InlineData(10001, 10000, false)]
    [InlineData(0, 100000, false)]
    [InlineData(-1, 100000, false)]
    public void Affordability_RequiresPositiveCostWithinLiveBalance(int cost, int balance, bool affordable)
    {
        Exception? failure = Record.Exception(() => RedemptionAssertions.AssertAffordable(cost, balance));
        if (affordable)
        {
            Assert.Null(failure);
        }
        else
        {
            Assert.NotNull(failure);
        }
    }
}
