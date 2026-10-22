using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.Core.Text;

namespace VAA.EAPI.Automation.MobileApp.UI.Assertions;

internal static class ReviewTotalAssertions
{
    internal static void VerifyCopy(string fareConditions, string guidance, string continueLabel)
    {
        Assert.Equal("Fare conditions and details", NormalizeCopy(fareConditions));
        Assert.Equal("Continue to secure payment", NormalizeCopy(continueLabel));
        const string title = "Enable international payments";
        const string message = "To ensure a smooth payment, check if your card is enabled for international or online payments.";
        string normalized = NormalizeCopy(guidance);
        // XCTest may separate combined child labels with a comma or a line break.
        Assert.True(normalized == title + ", " + message || normalized == title + " " + message,
            "Review total international-payment guidance differs from the approved English design copy.");
    }

    private static string NormalizeCopy(string text)
        => string.Join(" ", text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

    internal static void Verify(IReadOnlyList<string> rows, string summaryTotal, bool allowSeats,
        decimal? expectedSeatChargeGbp = null)
    {
        Amount baseline = ReadAmount(summaryTotal);
        Amount flights = ReadAmount(RequiredRow(rows, "Flights"));
        Amount taxes = ReadAmount(RequiredRow(rows, "Taxes, fees and charges"));
        Amount total = ReadAmount(RequiredRow(rows, "Total to pay"));
        string[] seatRows = rows.Where(row => row.StartsWith("Seat selection", StringComparison.Ordinal)).ToArray();
        Assert.True(seatRows.Length <= 1, "Review total exposes ambiguous seat-charge rows.");
        Assert.True(allowSeats || seatRows.Length == 0, "Unexpected seat charge on a no-seat checkout.");
        Amount seats = seatRows.Length == 0 ? new Amount(0, null, 0) : ReadAmount(seatRows[0]);
        if (allowSeats)
        {
            decimal expectedCharge = expectedSeatChargeGbp ?? throw new InvalidOperationException(
                "Selected-seat checkout requires independent seat-detail price evidence.");
            Assert.Equal("£", total.Currency);
            Assert.Equal(expectedCharge, seats.Cash);
            Assert.True(expectedCharge == 0 || seatRows.Length == 1,
                "A paid selected seat must have a Review total seat-charge row.");
        }
        Assert.True(total.Currency is not null, "Review total must expose a cash currency, including reward taxes.");
        foreach (Amount amount in new[] { baseline, flights, taxes, seats })
        {
            Assert.True(amount.Currency is null || amount.Currency == total.Currency,
                "Review total changed currency or mixes currencies in its breakdown.");
        }
        Assert.Equal(0, taxes.Points);
        Assert.Equal(0, seats.Points);
        Assert.Equal(flights.Cash + taxes.Cash + seats.Cash, total.Cash);
        Assert.Equal(flights.Points, total.Points);
        Assert.Equal(baseline.Points, total.Points);
        Assert.Equal(baseline.Cash + seats.Cash, total.Cash);
    }

    internal static void VerifyRetainedFigures(IReadOnlyList<string> before, IReadOnlyList<string> after)
    {
        foreach (string title in new[] { "Flights", "Taxes, fees and charges", "Total to pay" })
        {
            Amount expected = ReadAmount(RequiredRow(before, title));
            Amount actual = ReadAmount(RequiredRow(after, title));
            Assert.True(expected == actual,
                $"Review '{title}' changed after navigation: expected {expected}, actual {actual}.");
        }
        string[] beforeSeats = before.Where(row => row.StartsWith("Seat selection,", StringComparison.Ordinal) ||
            row.StartsWith("Seat selection ", StringComparison.Ordinal)).ToArray();
        string[] afterSeats = after.Where(row => row.StartsWith("Seat selection,", StringComparison.Ordinal) ||
            row.StartsWith("Seat selection ", StringComparison.Ordinal)).ToArray();
        Assert.True(beforeSeats.Length <= 1 && afterSeats.Length <= 1, "Ambiguous seat rows after navigation.");
        Assert.Equal(beforeSeats.Length, afterSeats.Length);
        if (beforeSeats.Length == 1)
        {
            Assert.Equal(ReadAmount(beforeSeats[0]), ReadAmount(afterSeats[0]));
        }
    }

    private static string RequiredRow(IReadOnlyList<string> rows, string title)
    {
        string[] matches = rows.Where(row => row.StartsWith(title + ",", StringComparison.Ordinal) ||
            row.StartsWith(title + " ", StringComparison.Ordinal)).ToArray();
        Assert.True(matches.Length == 1, $"Review total must expose exactly one '{title}' row.");
        return matches[0];
    }

    private static Amount ReadAmount(string text)
    {
        MatchCollection money = RegexCompiled.Matches(text, @"(?<currency>[£$€])\s*(?<cash>\d+(?:,\d{3})*(?:\.\d{2})?)(?![\d.,])");
        MatchCollection points = RegexCompiled.Matches(text, @"(?<points>\d+(?:,\d{3})*)\s+points\b", RegexOptions.IgnoreCase);
        Assert.True(money.Count <= 1 && points.Count <= 1 && money.Count + points.Count > 0,
            "Review/Summary amount is missing or ambiguous; cannot compare prices safely.");
        Assert.DoesNotContain("-", text);
        return new Amount(money.Count == 0 ? 0 : decimal.Parse(money[0].Groups["cash"].Value,
                NumberStyles.Number, CultureInfo.InvariantCulture),
            money.Count == 0 ? null : money[0].Groups["currency"].Value,
            points.Count == 0 ? 0 : int.Parse(points[0].Groups["points"].Value, NumberStyles.AllowThousands, CultureInfo.InvariantCulture));
    }

    private sealed record Amount(decimal Cash, string? Currency, int Points);
}
