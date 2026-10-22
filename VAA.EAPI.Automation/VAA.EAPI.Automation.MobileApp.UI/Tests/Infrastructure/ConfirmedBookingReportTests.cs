using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class ConfirmedBookingReportTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("SearchBook_TC48_GuestBooking")]
    [InlineData("LoyaltySearchBook_TC01_Redemption")]
    public void Format_EmitsSharedMarker(string scenario)
        => Assert.Equal($"[confirmed-booking] PNR=ABC123; scenario={scenario}",
            ConfirmedBookingReport.Format("ABC123", scenario));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("", "SearchBook_TC48_GuestBooking")]
    [InlineData("ABC123\nsecret", "SearchBook_TC48_GuestBooking")]
    [InlineData("ABC123", "SearchBook_TC48_GuestBooking\nsecret")]
    [InlineData("ABC123", "passenger@example.com")]
    public void Format_RejectsUnsafeValuesWithoutEchoingThem(string reference, string scenario)
    {
        ArgumentException exception = Assert.Throws<ArgumentException>(() => ConfirmedBookingReport.Format(reference, scenario));
        Assert.Equal("Confirmed-booking reporting requires a valid reference and scenario identifier.", exception.Message);
    }
}
