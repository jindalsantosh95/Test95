using VAA.EAPI.Automation.Core.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.Core.Playwright.HostedPayment;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Factory;

namespace VAA.EAPI.Automation.Core.UI.Helpers;

public static class HostedPaymentAutomation
{
    private const int DefaultTimeoutMs = 60_000;
    private const int DefaultMaxConcurrentPlaywrightSessions = 2;
    private const int DefaultMaxConcurrentSeleniumSessions = 2;
    private static readonly int MaxConcurrentPlaywrightSessions = GetMaxConcurrentPlaywrightSessions();
    private static readonly SemaphoreSlim PlaywrightSessionGate = new(
        MaxConcurrentPlaywrightSessions,
        MaxConcurrentPlaywrightSessions);
    private static readonly int MaxConcurrentSeleniumSessions = GetMaxConcurrentSeleniumSessions();
    private static readonly SemaphoreSlim SeleniumSessionGate = new(
        MaxConcurrentSeleniumSessions,
        MaxConcurrentSeleniumSessions);

    public static bool UsePlaywrightByDefault => ConfigReader.Get("UsePlaywrightForHpp", false);

    public static async Task ValidateAsync(
        TestConfig config,
        double addedSeatTotalPrice,
        bool rewardBooking = false,
        bool? headless = null)
    {
        await ValidateAsync(config.Book.Order.HppUrl, !rewardBooking && addedSeatTotalPrice <= 0, headless);
    }

    public static async Task ValidateAsync(string hppUrl, bool expectPayPal, bool? headless = null)
    {
        await PlaywrightSessionGate.WaitAsync();
        PlaywrightBrowserSession? session = null;
        try
        {
            session = await StartPlaywrightSessionAsync(hppUrl, headless);
            DirectHostedPaymentPage playwrightPaymentPage = DirectHostedPaymentPage.FromSession(session);
            await playwrightPaymentPage.ValidateLoadedAsync(
                expectPayPal,
                DefaultTimeoutMs);
            await session.MarkPassedAsync("Hosted payment page displayed the expected payment methods.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw session is null
                ? CreatePlaywrightLaunchFailureException("validation", exception)
                : await CreateHppFailureExceptionAsync(session, "validation", exception);
        }
        finally
        {
            await DisposeSessionAndReleasePermitAsync(session);
        }
    }

    public static async Task SubmitCardAsync(
        TestConfig config,
        bool? usePlaywright = null,
        bool? headless = null,
        double? expectedTotalPrice = null,
        bool rewardBooking = false)
    {
        if (!(usePlaywright ?? UsePlaywrightByDefault))
        {
            await SeleniumSessionGate.WaitAsync();
            try
            {
                using NavigateToHostedPaymentPage seleniumPaymentPage = new(config.Book.Order.HppUrl);
                if (expectedTotalPrice.HasValue)
                {
                    seleniumPaymentPage.ValidateHPPLoaded(expectedTotalPrice.Value, rewardBooking);
                }
                seleniumPaymentPage.SubmitHppCardDetails(config);
                return;
            }
            finally
            {
                SeleniumSessionGate.Release();
            }
        }

        await PlaywrightSessionGate.WaitAsync();
        PlaywrightBrowserSession? session = null;
        try
        {
            session = await StartPlaywrightSessionAsync(
                config.Book.Order.HppUrl,
                headless);
            DirectHostedPaymentPage playwrightPaymentPage = DirectHostedPaymentPage.FromSession(session);
            if (expectedTotalPrice.HasValue)
            {
                await playwrightPaymentPage.ValidateLoadedAsync(
                    expectPayPal: !rewardBooking && expectedTotalPrice.Value <= 0,
                    DefaultTimeoutMs);
            }
            await playwrightPaymentPage.SubmitCardDetailsAsync(config.Book.Payment.CardDetails, DefaultTimeoutMs);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw session is null
                ? CreatePlaywrightLaunchFailureException("card", exception)
                : await CreateHppFailureExceptionAsync(session, "card", exception);
        }
        finally
        {
            await DisposeSessionAndReleasePermitAsync(session);
        }
    }

    public static async Task<string> SubmitPayPalAsync(
        TestConfig config,
        bool? usePlaywright = null,
        bool? headless = null,
        double? expectedTotalPrice = null,
        bool rewardBooking = false)
    {
        if (!(usePlaywright ?? UsePlaywrightByDefault))
        {
            await SeleniumSessionGate.WaitAsync();
            try
            {
                using NavigateToHostedPaymentPage seleniumPaymentPage = new(config.Book.Order.HppUrl);
                if (expectedTotalPrice.HasValue)
                {
                    seleniumPaymentPage.ValidateHPPLoaded(expectedTotalPrice.Value, rewardBooking);
                }
                seleniumPaymentPage.SubmitPayPal(config);
                return seleniumPaymentPage.GetChildPri();
            }
            finally
            {
                SeleniumSessionGate.Release();
            }
        }

        await PlaywrightSessionGate.WaitAsync();
        PlaywrightBrowserSession? session = null;
        try
        {
            session = await StartPlaywrightSessionAsync(
                config.Book.Order.HppUrl,
                headless);
            DirectHostedPaymentPage playwrightPaymentPage = DirectHostedPaymentPage.FromSession(session);
            if (expectedTotalPrice.HasValue)
            {
                await playwrightPaymentPage.ValidateLoadedAsync(
                    expectPayPal: !rewardBooking && expectedTotalPrice.Value <= 0,
                    DefaultTimeoutMs);
            }
            return await playwrightPaymentPage.SubmitPayPalAsync(config.Book.Payment.APMDetails, DefaultTimeoutMs);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw session is null
                ? CreatePlaywrightLaunchFailureException("PayPal", exception)
                : await CreateHppFailureExceptionAsync(session, "PayPal", exception);
        }
        finally
        {
            await DisposeSessionAndReleasePermitAsync(session);
        }
    }

    public static async Task SubmitExistingPaymentAsync(
        TestConfig config,
        bool expandCard,
        bool? headless = null)
    {
        await PlaywrightSessionGate.WaitAsync();
        PlaywrightBrowserSession? session = null;
        try
        {
            session = await StartPlaywrightSessionAsync(
                config.Book.Order.HppUrl,
                headless);
            DirectHostedPaymentPage playwrightPaymentPage = DirectHostedPaymentPage.FromSession(session);
            await playwrightPaymentPage.SubmitExistingPaymentAsync(expandCard, DefaultTimeoutMs);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw session is null
                ? CreatePlaywrightLaunchFailureException("existing payment", exception)
                : await CreateHppFailureExceptionAsync(session, "existing payment", exception);
        }
        finally
        {
            await DisposeSessionAndReleasePermitAsync(session);
        }
    }

    private static async Task DisposeSessionAndReleasePermitAsync(PlaywrightBrowserSession? session)
    {
        try
        {
            if (session is not null)
            {
                await session.DisposeAsync();
            }
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.TraceWarning(
                $"Playwright HPP session cleanup failed: {exception.GetType().Name}.");
        }
        finally
        {
            PlaywrightSessionGate.Release();
        }
    }

    private static async Task<PlaywrightBrowserSession> StartPlaywrightSessionAsync(
        string hppUrl,
        bool? headless)
    {
        Uri secureHppUri = GetSecureHppUri(hppUrl);
        PlaywrightBrowserOptions options = new()
        {
            BaseUrl = secureHppUri,
            BrowserName = "chromium",
            BrowserChannel = "chrome",
            Headless = headless ?? GetHeadlessSetting(),
            // Direct-HPP traces can capture payment references, card fields or third-party
            // login interactions. Keep them disabled until trace redaction is implemented.
            TraceEnabled = false,
            RetainPassedArtifacts = false,
            // Some non-production HPP deployments have historically required this Chromium
            // workaround. It is opt-in and is never permitted for production monitoring.
            DisableWebSecurity = ConfigReader.Get("HppPlaywrightDisableWebSecurity", false) &&
                !ConfigReader.Get("env", string.Empty).Is("prod"),
            ActionTimeoutMs = DefaultTimeoutMs,
            NavigationTimeoutMs = DefaultTimeoutMs,
            OriginScopedHttpHeaders = BrowserFactory.HostedPaymentRequestHeaders,
            HeaderAllowedOrigins = GetHppHeaderAllowedOrigins(secureHppUri)
        };

        return await PlaywrightBrowserSession.StartAsync(options);
    }

    private static Uri GetSecureHppUri(string hppUrl)
    {
        if (!Uri.TryCreate(hppUrl, UriKind.Absolute, out Uri? uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            !uri.UserInfo.IsNullOrWhiteSpace() ||
            !IsAllowedHppPageOrigin(uri))
        {
            throw new InvalidOperationException(
                "The hosted payment URL must use an explicitly allowed HTTPS origin without user information.");
        }

        return uri;
    }

    private static bool IsAllowedHppPageOrigin(Uri hppUri)
    {
        return GetConfiguredExactOrigins("HPP_PAGE_ALLOWED_ORIGINS").Any(
            origin => origin.Scheme.Equals(hppUri.Scheme, StringComparison.OrdinalIgnoreCase) &&
                origin.Host.Equals(hppUri.Host, StringComparison.OrdinalIgnoreCase) &&
                origin.Port == hppUri.Port);
    }

    private static IReadOnlyCollection<Uri> GetConfiguredExactOrigins(string variableName)
    {
        string? configuredOrigins = Environment.GetEnvironmentVariable(variableName);
        if (configuredOrigins.IsNullOrWhiteSpace())
        {
            return [];
        }

        List<Uri> origins = [];
        foreach (string value in configuredOrigins.Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (!Uri.TryCreate(value, UriKind.Absolute, out Uri? origin) ||
                origin.Scheme != Uri.UriSchemeHttps ||
                !origin.UserInfo.IsNullOrWhiteSpace() ||
                !origin.GetLeftPart(UriPartial.Authority).TrimEnd('/').Equals(
                    value.TrimEnd('/'),
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"{variableName} must contain only exact HTTPS origins without user information, paths, queries or fragments.");
            }

            origins.Add(origin);
        }

        return origins;
    }

    private static IReadOnlyCollection<Uri> GetHppHeaderAllowedOrigins(Uri hppUri)
    {
        List<Uri> origins = [hppUri];
        foreach (Uri origin in GetConfiguredExactOrigins("HPP_HEADER_ALLOWED_ORIGINS"))
        {
            origins.Add(origin);
        }

        return origins;
    }

    private static async Task<InvalidOperationException> CreateHppFailureExceptionAsync(
        PlaywrightBrowserSession session,
        string paymentMethod,
        Exception innerException)
    {
        string pageOrigin = GetPageOrigin(session.Page.Url);
        string pageTitle = DiagnosticSanitizer.Sanitize(await ReadPageTitleAsync(session.Page));
        string screenshotPath;
        try
        {
            await RedactPaymentInputsAsync(session.Page);
            screenshotPath = await session.SaveScreenshotAsync($"hpp-{paymentMethod}-failure");
        }
        catch (Exception screenshotException)
        {
            screenshotPath = $"not captured ({screenshotException.GetType().Name})";
        }

        string safeCause = DiagnosticSanitizer.Sanitize(
            $"{innerException.GetType().Name}: {innerException.Message}");
        return new InvalidOperationException(
            $"Playwright HPP {paymentMethod} interaction failed. "
            + $"PageOrigin={pageOrigin}; PageTitle={pageTitle}; Screenshot={screenshotPath}. "
            + $"Cause={safeCause}. "
            + "The URL query and request headers are intentionally excluded from diagnostics.");
    }

    private static InvalidOperationException CreatePlaywrightLaunchFailureException(
        string paymentMethod,
        Exception exception)
    {
        if (exception is Microsoft.Playwright.PlaywrightException
            && exception.ToString().Contains("run out of resources", StringComparison.OrdinalIgnoreCase))
        {
            return new InvalidOperationException(
                $"Playwright HPP {paymentMethod} could not start Chrome because the test runner exhausted its resources. "
                + $"Concurrent HPP browser sessions are capped at {MaxConcurrentPlaywrightSessions}. "
                + $"Cause={DiagnosticSanitizer.Sanitize(exception.Message)}.");
        }

        return new InvalidOperationException(
            $"Playwright HPP {paymentMethod} failed before page diagnostics could be captured. "
            + $"Cause={DiagnosticSanitizer.Sanitize($"{exception.GetType().Name}: {exception.Message}")}.");
    }

    private static int GetMaxConcurrentPlaywrightSessions()
    {
        const string settingName = "HPP_PLAYWRIGHT_MAX_CONCURRENT_SESSIONS";
        string? configuredValue = System.Environment.GetEnvironmentVariable(settingName);
        return int.TryParse(configuredValue, out int configuredLimit) && configuredLimit is >= 1 and <= 8
            ? configuredLimit
            : DefaultMaxConcurrentPlaywrightSessions;
    }

    private static int GetMaxConcurrentSeleniumSessions()
    {
        const string settingName = "HPP_SELENIUM_MAX_CONCURRENT_SESSIONS";
        string? configuredValue = System.Environment.GetEnvironmentVariable(settingName);
        return int.TryParse(configuredValue, out int configuredLimit) && configuredLimit is >= 1 and <= 8
            ? configuredLimit
            : DefaultMaxConcurrentSeleniumSessions;
    }

    private static async Task RedactPaymentInputsAsync(Microsoft.Playwright.IPage page)
    {
        const string clearInputValues = "elements => { "
            + "elements.forEach(element => { element.value = ''; element.setAttribute('value', ''); }); "
            + "return elements.length; }";

        foreach (Microsoft.Playwright.IFrame frame in page.Frames)
        {
            await frame.Locator("input, textarea").EvaluateAllAsync<int>(clearInputValues);
        }
    }

    private static string GetPageOrigin(string pageUrl)
    {
        return Uri.TryCreate(pageUrl, UriKind.Absolute, out Uri? uri)
            ? uri.GetLeftPart(UriPartial.Authority)
            : "unavailable";
    }

    private static async Task<string> ReadPageTitleAsync(Microsoft.Playwright.IPage page)
    {
        try
        {
            string title = await page.TitleAsync();
            return title.IsNullOrWhiteSpace() ? "empty" : title.ReplaceLineEndings(" ").Trim();
        }
        catch (Exception exception)
        {
            return $"unavailable ({exception.GetType().Name})";
        }
    }

    private static bool GetHeadlessSetting()
    {
        string sharedSetting = ConfigReader.Get("HppPlaywrightHeadless");
        return bool.TryParse(sharedSetting, out bool isHeadless)
            ? isHeadless
            : ConfigReader.Get("BookHppPlaywrightHeadless", true);
    }
}
