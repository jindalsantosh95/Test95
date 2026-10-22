using System.Drawing;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class HppInputPolicyTests
{
    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(null, "Crawley", true, true)]
    [InlineData(null, "Town / City", false, true)]
    [InlineData(null, null, false, true)]
    [InlineData("", "Crawley", false, false)]
    [InlineData("Town / City", "Crawley", false, false)]
    [InlineData("Crawley", "Town / City", true, false)]
    public void ValueCheck_ReadsTextOnlyWhenNativeValueIsAbsent(
        string? nativeValue, string? text, bool expected, bool shouldReadText)
    {
        bool textRead = false;
        Assert.Equal(expected, NativeHostedPaymentPage.HppValueMatches(nativeValue, "Crawley", () =>
        {
            textRead = true;
            return text;
        }));
        Assert.Equal(shouldReadText, textRead);
    }

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData("Crawley", true)]
    [InlineData("crawley", true)]
    [InlineData("Town / City", false)]
    [InlineData("1 Test StreetCrawley", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void ValueCheck_RequiresRetainedInputNotPlaceholderOrOtherField(string? observed, bool expected)
        => Assert.Equal(expected, NativeHostedPaymentPage.HppValueMatches(observed, "Crawley"));

    [Theory]
    [Trait("Category", "MobileApp-Offline")]
    [InlineData(20, 100, 300, 40, true)]
    [InlineData(20, 790, 300, 40, false)]
    [InlineData(20, -10, 300, 40, false)]
    [InlineData(20, 100, 0, 40, false)]
    [InlineData(390, 100, 30, 40, false)]
    public void InputBounds_RequireCompleteOnscreenTarget(int x, int y, int width, int height, bool expected)
        => Assert.Equal(expected, NativeHostedPaymentPage.HppBoundsAreSafe(
            new Rectangle(x, y, width, height), new Size(400, 800)));
}
