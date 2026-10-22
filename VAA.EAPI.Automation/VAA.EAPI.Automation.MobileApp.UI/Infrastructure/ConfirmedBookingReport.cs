using System.Text.RegularExpressions;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

internal static class ConfirmedBookingReport
{
    internal static string Format(string bookingReference, string testName)
    {
        if (!Regex.IsMatch(bookingReference, "\\A[A-Z0-9]{6}\\z", RegexOptions.CultureInvariant) ||
            !Regex.IsMatch(testName, "\\A(?:Loyalty)?SearchBook_TC[0-9]+_[A-Za-z0-9_]+\\z", RegexOptions.CultureInvariant))
        {
            throw new ArgumentException("Confirmed-booking reporting requires a valid reference and scenario identifier.");
        }

        return $"[confirmed-booking] PNR={bookingReference}; scenario={testName}";
    }
}
