using VAA.EAPI.Automation.Core.Playwright.Browser;
using Xunit;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Support;

public sealed class ApplicationNavigationTests
{
    private static readonly Uri BaseUrl = new("https://stg.virginatlantic.com/");
    private const string SearchUrl =
        "/flights/search/slice?origin=LHR&destination=JFK&passengers=a1t0c0i0";

    [Fact]
    public void HomepageInterruption_IsRetryable()
    {
        bool retryable = ApplicationNavigation.IsRetryableHomepageInterruption(
            "Navigation to the search route is interrupted by another navigation to the homepage",
            BaseUrl.ToString(),
            BaseUrl,
            SearchUrl);

        Assert.True(retryable);
    }

    [Theory]
    [InlineData("Navigation timed out", "https://stg.virginatlantic.com/")]
    [InlineData("is interrupted by another navigation to", "https://stg.virginatlantic.com/account")]
    [InlineData("is interrupted by another navigation to", "https://example.com/")]
    public void OtherFailures_AreNotRetryable(string message, string currentUrl)
    {
        bool retryable = ApplicationNavigation.IsRetryableHomepageInterruption(
            message,
            currentUrl,
            BaseUrl,
            SearchUrl);

        Assert.False(retryable);
    }

    [Fact]
    public void CrossOriginTarget_IsNotRetryable()
    {
        bool retryable = ApplicationNavigation.IsRetryableHomepageInterruption(
            "is interrupted by another navigation to",
            BaseUrl.ToString(),
            BaseUrl,
            "https://example.com/flights/search/slice");

        Assert.False(retryable);
    }
}
