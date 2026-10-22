using System.Text.RegularExpressions;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class TripsReferenceTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Booking Reference: AB12CD", true)]
    [InlineData("Your trip to New York, Booking Reference: A B 1 2 C D, 91 days", true)]
    [InlineData("ab12cd", true)]
    [InlineData("AB12CE", false)]
    [InlineData("A B 1 2 C E", false)]
    [InlineData("XAB12CD", false)]
    [InlineData("AB12CDX", false)]
    [InlineData("Your trip to New York", false)]
    public void ReferencePattern_MatchesCompactAndSpokenReferenceOnly(string observed, bool expected)
        => Assert.Equal(expected, Regex.IsMatch(observed, TripsPage.BookingReferencePattern("AB12CD"),
            RegexOptions.IgnoreCase));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("")]
    [InlineData("ABCDE")]
    [InlineData("ABCDEFG")]
    [InlineData("AB'123")]
    public void ReferencePattern_RejectsInvalidPredicateInput(string reference)
        => Assert.Throws<ArgumentException>(() => TripsPage.BookingReferencePattern(reference));
}
