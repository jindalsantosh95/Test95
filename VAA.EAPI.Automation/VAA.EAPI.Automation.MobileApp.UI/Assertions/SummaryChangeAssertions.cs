using System.Globalization;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Text;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

public static class SummaryChangeAssertions
{
    // Existing English result-card schedule contract. Do not use row indexes or prices as flight identity.
    public static string Schedule(string text)
    {
        string[] times = RegexCompiled.Matches(text, @"\b\d{1,2}:\d{2}\b")
            .Select(match => match.Value).Distinct(StringComparer.Ordinal).Take(2).ToArray();
        if (times.Length != 2)
        {
            throw new InvalidOperationException("Flight card must expose departure and arrival times before selecting a replacement.");
        }
        return string.Join("|", times);
    }

    internal static (string Currency, decimal Amount) Money(string text)
    {
        (string Currency, decimal Amount)[] amounts = RegexCompiled.Matches(text,
                @"(?<currency>[$£€])\s*(?<amount>(?:\d{1,3}(?:,\d{3})+|\d+)(?:\.\d{2})?)(?![\d.,])")
            .Select(match => (match.Groups["currency"].Value,
                decimal.Parse(match.Groups["amount"].Value, NumberStyles.Number, CultureInfo.InvariantCulture)))
            .Distinct().ToArray();
        if (amounts.Length != 1 || amounts[0].Amount <= 0)
        {
            throw new InvalidOperationException("Expected one unambiguous positive currency amount for the single-adult fare/summary.");
        }
        return amounts[0];
    }

    public static void MatchesReplacement(SelectedFlight original, SelectedFlight replacement,
        string summaryFareTitle, string summaryPrice)
    {
        Assert.NotEqual(Schedule(original.ResultText), Schedule(replacement.ResultText));
        Assert.False(replacement.FareTitle.IsNullOrWhiteSpace(), "Replacement fare title must be observed before selection.");
        Assert.Equal(replacement.FareTitle.Trim(), summaryFareTitle.Trim(), ignoreCase: true);
        Assert.Equal(Money(replacement.FareLabel), Money(summaryPrice));
    }

    internal static (string Currency, decimal Amount) ReturnTotal(string outboundLabel, string inboundLabel)
    {
        (string currency, decimal outbound) = Money(outboundLabel);
        (string Sign, string Currency, decimal Amount)[] prices = RegexCompiled.Matches(inboundLabel,
                @"(?<sign>[+−-]?)\s*(?<currency>[$£€])\s*(?<amount>(?:\d{1,3}(?:,\d{3})+|\d+)(?:\.\d{2})?)(?![\d.,])")
            .Select(match => (match.Groups["sign"].Value, match.Groups["currency"].Value,
                decimal.Parse(match.Groups["amount"].Value, NumberStyles.Number, CultureInfo.InvariantCulture)))
            .Distinct().ToArray();
        if (prices.Length != 1 || !prices[0].Currency.Is(currency))
        {
            throw new InvalidOperationException("Inbound fare requires one amount in the outbound currency.");
        }
        decimal total = prices[0].Sign switch
        {
            "+" => outbound + prices[0].Amount,
            "-" or "−" => outbound - prices[0].Amount,
            _ => prices[0].Amount,
        };
        if (total <= 0)
            throw new InvalidOperationException("Return total must be positive.");
        return (currency, total);
    }
}
