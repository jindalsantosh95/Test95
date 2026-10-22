using System.Globalization;
using System.Text.RegularExpressions;

namespace VAA.EAPI.Automation.MobileApp.UI.Models;

internal static class SeatPriceEvidence
{
    internal static decimal ReadDetailGbp(string text)
        => text.Trim() == "Seat price: Free" ? 0 : ReadGbp(text);

    // UK-origin revenue scenario: never infer currency or treat missing evidence as zero.
    internal static decimal ReadGbp(string text)
    {
        MatchCollection matches = Regex.Matches(text,
            @"(?<sign>[+−-]?)\s*(?<currency>[$£€])\s*(?<amount>(?:\d{1,3}(?:,\d{3})+|\d+)(?:\.\d{2})?)(?![\d.,])",
            RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        if (matches.Count != 1 || matches[0].Groups["currency"].Value != "£" ||
            matches[0].Groups["sign"].Value is "-" or "−")
            throw new InvalidOperationException("Seat pricing requires exactly one non-negative GBP amount in the observed label.");
        return decimal.Parse(matches[0].Groups["amount"].Value, NumberStyles.Number, CultureInfo.InvariantCulture);
    }
}
