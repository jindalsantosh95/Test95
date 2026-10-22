using Microsoft.Playwright;

namespace VAA.EAPI.Automation.Core.Playwright.Browser;

public static class ApplicationNavigation
{
    private const string InterruptedNavigationMessage = "is interrupted by another navigation to";

    public static async Task<IResponse?> NavigateAsync(
        IPage page,
        Uri baseUrl,
        string targetUrl,
        WaitUntilState waitUntil,
        int timeoutMs)
    {
        PageGotoOptions options = new()
        {
            WaitUntil = waitUntil,
            Timeout = timeoutMs
        };

        try
        {
            return await page.GotoAsync(targetUrl, options);
        }
        catch (PlaywrightException exception) when (
            IsRetryableHomepageInterruption(exception.Message, page.Url, baseUrl, targetUrl))
        {
            Console.WriteLine(
                $"[navigation-retry] The application homepage interrupted navigation to '{targetUrl}'. " +
                "Waiting for the bootstrap navigation to settle before retrying once.");

            try
            {
                await page.WaitForLoadStateAsync(
                    LoadState.DOMContentLoaded,
                    new PageWaitForLoadStateOptions { Timeout = Math.Min(timeoutMs, 15_000) });
            }
            catch (TimeoutException)
            {
                // The retry below remains the authoritative navigation readiness check.
            }

            await page.WaitForTimeoutAsync(750);
            return await page.GotoAsync(targetUrl, options);
        }
    }

    public static bool IsRetryableHomepageInterruption(
        string exceptionMessage,
        string currentUrl,
        Uri baseUrl,
        string targetUrl)
    {
        if (!exceptionMessage.Contains(InterruptedNavigationMessage, StringComparison.OrdinalIgnoreCase) ||
            !Uri.TryCreate(baseUrl, targetUrl, out Uri? target) ||
            !Uri.TryCreate(currentUrl, UriKind.Absolute, out Uri? current))
        {
            return false;
        }

        bool currentIsSameApplication = current.Scheme.Equals(baseUrl.Scheme, StringComparison.OrdinalIgnoreCase) &&
            current.Host.Equals(baseUrl.Host, StringComparison.OrdinalIgnoreCase) &&
            current.Port == baseUrl.Port;
        bool targetIsSameApplication = target.Scheme.Equals(baseUrl.Scheme, StringComparison.OrdinalIgnoreCase) &&
            target.Host.Equals(baseUrl.Host, StringComparison.OrdinalIgnoreCase) &&
            target.Port == baseUrl.Port;
        bool isHomepage = current.AbsolutePath.Equals("/", StringComparison.Ordinal) &&
            string.IsNullOrEmpty(current.Query);
        bool targetIsDifferentRoute = !target.PathAndQuery.Equals(current.PathAndQuery, StringComparison.Ordinal);

        return currentIsSameApplication && targetIsSameApplication && isHomepage && targetIsDifferentRoute;
    }
}
