using VAA.EAPI.Automation.MobileApp.UI.Assertions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class SummaryUpgradeAssertionsTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(true, "+£100", "£600", true)]
    [InlineData(false, "£600", "£600", true)]
    [InlineData(false, "£600", "£1100", false)]
    [InlineData(true, "+£100", "£100", false)]
    [InlineData(true, "+£100", "£500", false)]
    [InlineData(true, "+$100", "£600", false)]
    [InlineData(true, "-£100", "£400", false)]
    [InlineData(true, "£0", "£500", false)]
    [InlineData(false, "£400", "£400", false)]
    public void UpgradePrice_DistinguishesDifferenceFromTotalAndRejectsStaleOrUnsupportedValues(
        bool difference, string price, string actual, bool valid)
    {
        SummaryUpgradeOffer offer = new(0, "Premium", price, difference);
        Exception? failure = Record.Exception(() => SummaryUpgradeAssertions.MatchesOffer(
            "Economy Classic", "£500", offer, "Premium", actual));
        if (valid)
        {
            Assert.Null(failure);
        }
        else
        {
            Assert.NotNull(failure);
        }
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Economy Classic", "Economy Classic")]
    [InlineData("Premium", "Premium")]
    public void UpgradeFare_RejectsUnchangedOrWrongFare(string original, string actual)
    {
        SummaryUpgradeOffer offer = new(1, "Premium", "£100", true);
        Assert.NotNull(Record.Exception(() => SummaryUpgradeAssertions.MatchesOffer(
            original, "£500", offer, actual, "£600")));
    }

    [Fact]
    [Trait("Category", "MobileApp-Offline")]
    public void UpgradeTitle_ReadsExactAppFormat()
        => Assert.Equal("Economy Delight", SummaryUpgradeAssertions.FareTitle("Upgrade trip to Economy Delight"));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("Upgrade trip to ")]
    [InlineData("Premium")]
    public void UpgradeTitle_RejectsMissingOrChangedContract(string text)
        => Assert.Throws<InvalidOperationException>(() => SummaryUpgradeAssertions.FareTitle(text));
}
