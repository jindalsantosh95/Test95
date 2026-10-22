using VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class FailureScreenshotPolicyTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("seatSelection.error", "XCUIElementTypeStaticText", true)]
    [InlineData("NativeFlightPaymentRetryButton", "XCUIElementTypeButton", true)]
    [InlineData("seatSelection.error", "XCUIElementTypeOther", false)]
    [InlineData("seatSelection.error", "XCUIElementTypeTextField", false)]
    [InlineData("PassengerDetailsHub", "XCUIElementTypeStaticText", false)]
    [InlineData("seatSelection.error", null, false)]
    public void FailureScreenshots_OnlyApprovedErrorControlsAreEligible(string identifier, string? type, bool expected)
        => Assert.Equal(expected, MobileFailureEvidence.CanCaptureErrorControl(identifier, type));
}
