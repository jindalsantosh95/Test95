using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

public static class SummaryUpgradeAssertions
{
    internal static string FareTitle(string label)
    {
        // App-owned English title format; fail if it changes rather than guessing a cabin.
        const string prefix = "Upgrade trip to ";
        if (!label.StartsWith(prefix, StringComparison.Ordinal) || label[prefix.Length..].IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("Summary upgrade did not expose the expected target-fare title.");
        }
        return label[prefix.Length..].Trim();
    }

    public static (string Currency, decimal Amount) ExpectedTotal(string originalPrice, SummaryUpgradeOffer offer)
    {
        if (offer.PriceText.Contains('-') || offer.PriceText.Contains('−'))
        {
            throw new InvalidOperationException("Negative upgrade pricing is outside this positive currency-upgrade scenario.");
        }
        (string currency, decimal original) = SummaryChangeAssertions.Money(originalPrice);
        (string upgradeCurrency, decimal upgrade) = SummaryChangeAssertions.Money(offer.PriceText);
        Assert.Equal(currency, upgradeCurrency);
        decimal expected = offer.IsDifference ? original + upgrade : upgrade;
        Assert.True(expected > original, "The offered upgrade must increase the total for this scenario.");
        return (currency, expected);
    }

    public static void MatchesOffer(string originalFare, string originalPrice, SummaryUpgradeOffer offer,
        string actualFare, string actualPrice)
    {
        Assert.False(originalFare.Is(offer.FareTitle), "Upgrade must target a different fare.");
        Assert.Equal(offer.FareTitle, actualFare.Trim(), ignoreCase: true);
        Assert.Equal(ExpectedTotal(originalPrice, offer), SummaryChangeAssertions.Money(actualPrice));
    }
}
