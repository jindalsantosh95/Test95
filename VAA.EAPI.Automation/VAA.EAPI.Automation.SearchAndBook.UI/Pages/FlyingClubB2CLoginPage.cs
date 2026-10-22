using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Playwright.Browser;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Pages;

public sealed class FlyingClubB2CLoginPage : BasePage
{
    public FlyingClubB2CLoginPage(IPage page)
        : base(page)
    {
    }

    public async Task SignInAsync(string accountUrl, string flyingClubNumber, string password, int timeoutMs)
    {
        Uri expectedApplicationUrl = ResolveExpectedApplicationUrl(accountUrl);
        TaskCompletionSource<string> memberDetailsFailure = new(TaskCreationOptions.RunContinuationsAsynchronously);

        async void CaptureMemberDetailsFailure(object? _, IResponse response)
        {
            try
            {
                string? requestBody = response.Request.PostData;
                if (!response.Url.Contains("/flights/search/api/graphql", StringComparison.OrdinalIgnoreCase) ||
                    requestBody is null ||
                    !requestBody.Contains("AccountMemberDetails", StringComparison.Ordinal))
                {
                    return;
                }

                string responseBody = await response.TextAsync();
                if (responseBody.Contains("\"errors\"", StringComparison.Ordinal))
                {
                    memberDetailsFailure.TrySetResult(responseBody.Length > 1_000 ? responseBody[..1_000] : responseBody);
                }
            }
            catch (Exception)
            {
                // Best-effort diagnostics must never affect the sign-in journey.
            }
        }

        Page.Response += CaptureMemberDetailsFailure;

        try
        {
            await ApplicationNavigation.NavigateAsync(
                Page,
                new Uri(expectedApplicationUrl.GetLeftPart(UriPartial.Authority)),
                accountUrl,
                WaitUntilState.DOMContentLoaded,
                timeoutMs);
            ILocator username = Page.Locator("#signInName");
            await username.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = timeoutMs });

            ILocator consent = Page.Locator("#ensAcceptAll");
            if (await consent.IsVisibleAsync())
            {
                await consent.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
            }

            await username.FillAsync(flyingClubNumber, new LocatorFillOptions { Timeout = timeoutMs });
            await Page.Locator("#password").FillAsync(password, new LocatorFillOptions { Timeout = timeoutMs });
            await Page.Locator("#continue").ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });

            await Page.WaitForURLAsync(
                urlText => IsExpectedApplicationRoute(urlText, expectedApplicationUrl),
                new PageWaitForURLOptions { Timeout = timeoutMs, WaitUntil = WaitUntilState.DOMContentLoaded });

            await Task.Delay(5_000);
            if (!IsExpectedApplicationRoute(Page.Url, expectedApplicationUrl))
            {
                string sourceFailure = memberDetailsFailure.Task.IsCompletedSuccessfully
                    ? await memberDetailsFailure.Task
                    : "not captured";
                throw new InvalidOperationException(
                    "B2C accepted the credentials, but the application rejected the authenticated session and initiated another login challenge. " +
                    $"AccountMemberDetails failure: {sourceFailure}");
            }
        }
        catch (TimeoutException exception)
        {
            string title = await Page.TitleAsync();
            string body = await Page.Locator("body").InnerTextAsync(new LocatorInnerTextOptions { Timeout = 5_000 });
            string sample = body.Length > 1_000 ? body[..1_000] : body;
            string safeUrl = Page.Url.Split('#')[0];
            throw new InvalidOperationException(
                $"B2C sign-in did not redirect to the expected application route. " +
                $"Expected host: {expectedApplicationUrl.Host}; URL: {safeUrl}; title: {title}; body: {sample}",
                exception);
        }
        finally
        {
            Page.Response -= CaptureMemberDetailsFailure;
        }
    }

    public async Task SignInFromHomeAsync(
        string homeUrl,
        string flyingClubNumber,
        string password,
        int timeoutMs)
    {
        Uri applicationUri = new(homeUrl);
        await Page.GotoAsync(homeUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = timeoutMs
        });
        await VirginAtlanticConsent.DismissPrivacyBannerIfPresentAsync(Page);

        System.Text.RegularExpressions.Regex loginLabel = new(
            "^(Log in|Sign in)$",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        ILocator loginCandidates = Page
            .Locator("a, button, [role='link'], [role='button']")
            .Filter(new LocatorFilterOptions { HasTextRegex = loginLabel });
        ILocator login = loginCandidates.First;

        await login.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await login.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        ILocator username = Page.Locator("#signInName");
        try
        {
            await Page.WaitForFunctionAsync(
                @"() => {
                    const visible = element => {
                        const rect = element.getBoundingClientRect();
                        const style = getComputedStyle(element);
                        return rect.width > 0 && rect.height > 0 &&
                            style.display !== 'none' && style.visibility !== 'hidden';
                    };
                    if ([...document.querySelectorAll('#signInName')].some(visible)) return true;
                    return [...document.querySelectorAll('a, button, [role=""link""], [role=""button""]')]
                        .filter(visible)
                        .filter(element => /^(Log in|Sign in)$/i.test((element.textContent || '').trim()))
                        .length > 1;
                }",
                null,
                new PageWaitForFunctionOptions { Timeout = Math.Min(timeoutMs, 15_000) });
        }
        catch (TimeoutException)
        {
            // The dedicated username wait below provides the final login readiness failure.
        }

        if (!await username.IsVisibleAsync() && await loginCandidates.CountAsync() > 1)
        {
            await loginCandidates.Last.ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });
        }
        await username.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeoutMs
        });
        await username.FillAsync(flyingClubNumber);
        await Page.Locator("#password").FillAsync(password);
        await Page.Locator("#continue").ClickAsync(new LocatorClickOptions { Timeout = timeoutMs });

        await Page.WaitForURLAsync(
            urlText => Uri.TryCreate(urlText, UriKind.Absolute, out Uri? url) &&
                url.Host.Equals(applicationUri.Host, StringComparison.OrdinalIgnoreCase) &&
                !url.Host.Contains("b2clogin.com", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = timeoutMs, WaitUntil = WaitUntilState.DOMContentLoaded });
    }

    private static bool IsExpectedApplicationRoute(string urlText, Uri expectedApplicationUrl)
    {
        return Uri.TryCreate(urlText, UriKind.Absolute, out Uri? url) &&
            url.Host.Equals(expectedApplicationUrl.Host, StringComparison.OrdinalIgnoreCase);
    }

    private static Uri ResolveExpectedApplicationUrl(string accountUrl)
    {
        if (Uri.TryCreate(accountUrl, UriKind.Absolute, out Uri? absoluteUrl) &&
            (absoluteUrl.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ||
             absoluteUrl.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase)) &&
            !StringExtensions.IsNullOrWhiteSpace(absoluteUrl.Host))
        {
            return absoluteUrl;
        }

        string? baseUrl = Environment.GetEnvironmentVariable("SAB_BASE_URL");
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri? applicationBaseUrl))
        {
            throw new InvalidOperationException(
                "SAB_BASE_URL must be an absolute URL when the B2C account URL is relative.");
        }

        return new Uri(applicationBaseUrl, accountUrl);
    }
}
