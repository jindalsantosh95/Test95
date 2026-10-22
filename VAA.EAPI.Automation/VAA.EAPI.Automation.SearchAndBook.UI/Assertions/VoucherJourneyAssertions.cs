using System.Globalization;
using System.Text.RegularExpressions;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Assertions;

public static class VoucherJourneyAssertions
{
    public static void AssertPriceRecalculated(
        PriceSummarySnapshot before,
        PriceSummarySnapshot after,
        string action)
    {
        BookingPriceAssertions.AssertValid(before, $"Before {action}");
        BookingPriceAssertions.AssertValid(after, $"After {action}");
        if (!BookingPriceAssertions.CurrenciesAreEquivalent(before.Currency, after.Currency))
        {
            throw new InvalidOperationException(
                $"Voucher {action} changed the currency from '{before.Currency}' to '{after.Currency}'.");
        }

        if (before.Total == after.Total && before.Points == after.Points)
        {
            throw new InvalidOperationException(
                $"Voucher {action} did not recalculate the visible price summary. " +
                $"Before: {before.Text}; after: {after.Text}");
        }
    }

    public static void AssertTwoEarliestExpiriesDisplayed(
        IReadOnlyList<DateOnly> accountExpiryDates,
        IReadOnlyList<string> displayedExpiryDates)
    {
        DateOnly[] expected = accountExpiryDates
            .Distinct()
            .OrderBy(date => date)
            .Take(2)
            .ToArray();
        if (expected.Length != 2)
        {
            throw new InvalidOperationException(
                "The selected test account did not contain two distinct voucher expiry dates.");
        }

        DateOnly[] actual = displayedExpiryDates
            .Select(ParseDate)
            .Where(date => date.HasValue)
            .Select(date => date.GetValueOrDefault())
            .Distinct()
            .OrderBy(date => date)
            .ToArray();
        if (!actual.SequenceEqual(expected))
        {
            throw new InvalidOperationException(
                $"Payment did not display only the two earliest voucher expiries. " +
                $"Expected: {string.Join(", ", expected)}; displayed: {string.Join(", ", displayedExpiryDates)}.");
        }
    }

    public static void AssertConfirmationBreakdown(ConfirmationDetails confirmation)
    {
        bool namedBenefit = Regex.IsMatch(
            confirmation.VoucherBenefit,
            "companion|upgrade|vouchers? redeemed",
            RegexOptions.IgnoreCase);
        if (!namedBenefit ||
            !Regex.IsMatch(confirmation.VoucherBenefit, @"\d[\d,]*\s+(?:Virgin\s+)?Points", RegexOptions.IgnoreCase))
        {
            throw new InvalidOperationException(
                $"Confirmation did not show the redeemed voucher benefit and covered points. " +
                $"Voucher benefit: '{confirmation.VoucherBenefit}'.");
        }

        if (!Regex.IsMatch(confirmation.PointsUsed, @"\d") ||
            !confirmation.PointsUsed.Contains("points", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Confirmation did not show the Virgin Points used. " +
                $"Points summary: '{confirmation.PointsUsed}'.");
        }
    }

    private static DateOnly? ParseDate(string value)
    {
        string[] formats =
        [
            "d/M/yyyy",
            "dd/MM/yyyy",
            "d-M-yyyy",
            "dd-MM-yyyy",
            "d MMM yyyy",
            "dd MMM yyyy",
            "d MMMM yyyy",
            "dd MMMM yyyy",
            "yyyy-MM-dd"
        ];
        return DateOnly.TryParseExact(
            value,
            formats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AllowWhiteSpaces,
            out DateOnly parsed)
            ? parsed
            : null;
    }
}
