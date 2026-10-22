using System.Globalization;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.SearchAndBook.UI.Pages;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Assertions;

public static class BookingPriceAssertions
{
    public static void AssertValid(PriceSummarySnapshot snapshot, string pageName)
    {
        if (snapshot.Text.IsNullOrWhiteSpace() || snapshot.Total.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException(
                $"{pageName} did not show a visible price summary with a total. Summary: {snapshot.Text}");
        }

        if (!decimal.TryParse(snapshot.Total, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal total) || total <= 0)
        {
            throw new InvalidOperationException(
                $"{pageName} price summary total was not a positive amount: '{snapshot.Total}'.");
        }
    }

    public static void AssertConsistent(
        PriceSummarySnapshot expected,
        PriceSummarySnapshot actual,
        string pageName)
    {
        AssertValid(expected, "Initial");
        AssertValid(actual, pageName);

        if (!CurrenciesAreEquivalent(expected.Currency, actual.Currency) ||
            expected.Total != actual.Total ||
            expected.Points != actual.Points)
        {
            throw new InvalidOperationException(
                $"{pageName} price summary changed from {expected} to {actual}. " +
                $"Initial summary: {expected.Text}; {pageName} summary: {actual.Text}");
        }
    }

    public static bool CurrenciesAreEquivalent(string expected, string actual)
    {
        if (expected.Equals(actual, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return expected.EndsWith('$') && actual == "$" ||
               actual.EndsWith('$') && expected == "$";
    }
}
