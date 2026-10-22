using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Microsoft.Playwright;
using VAA.EAPI.Automation.Core.Diagnostics;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Playwright.Browser;

public sealed class PlaywrightBrowserSession : IAsyncDisposable
{
    private bool _passed;
    private readonly IPlaywright _playwright;
    private readonly IBrowser? _browser;
    private readonly IAndroidDevice? _androidDevice;
    private readonly PlaywrightBrowserOptions _options;
    private readonly ConcurrentQueue<NetworkDiagnostic> _networkDiagnostics = new();
    private int _browserStackStatusPublished;
    private string _currentAction = "starting the browser session";
    private string? _failureContext;
    private string? _browserStackEgressIp;

    private PlaywrightBrowserSession(
        IPlaywright playwright,
        IBrowser? browser,
        IAndroidDevice? androidDevice,
        IBrowserContext context,
        IPage page,
        PlaywrightBrowserOptions options)
    {
        _playwright = playwright;
        _browser = browser;
        _androidDevice = androidDevice;
        Context = context;
        Page = page;
        _options = options;
    }

    public IBrowserContext Context { get; }

    public IPage Page { get; }

    public string ExecutionSummary =>
        _options.BrowserStack is null
            ? $"Execution: Local; Browser: {_options.BrowserName}; Headless: {_options.Headless}; " +
              $"Base URL: {SanitizeUrl(_options.BaseUrl.ToString())}; Artifacts: {_options.ArtifactDirectory}"
            : $"Execution: BrowserStack; Browser: {_options.BrowserStack.Browser} {_options.BrowserStack.BrowserVersion}; " +
              $"OS: {_options.BrowserStack.OperatingSystem} {_options.BrowserStack.OperatingSystemVersion}; " +
              $"Device: {_options.BrowserStack.DeviceName ?? "Desktop"}; Project: {_options.BrowserStack.ProjectName}; " +
              $"Build: {_options.BrowserStack.BuildName}; Session: {_options.BrowserStack.SessionName}; " +
              $"Local: {_options.BrowserStack.UseLocal}; Egress IP: {_browserStackEgressIp ?? "not captured"}; " +
              $"Base URL: {SanitizeUrl(_options.BaseUrl.ToString())}";

    public static async Task<PlaywrightBrowserSession> StartAsync(PlaywrightBrowserOptions options)
    {
        options = options with
        {
            ArtifactDirectory = Path.Combine(
                options.ArtifactDirectory,
                $"session-{DateTime.UtcNow:yyyyMMdd-HHmmssfff}-{Guid.NewGuid():N}")
        };
        Directory.CreateDirectory(options.ArtifactDirectory);

        IPlaywright playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        BrowserNewContextOptions contextOptions = new()
        {
            BaseURL = options.BaseUrl.ToString()
        };

        if (options.BrowserStack?.DeviceName is null)
        {
            contextOptions.ViewportSize = new ViewportSize
            {
                Width = options.ViewportWidth,
                Height = options.ViewportHeight
            };
        }

        IBrowser? browser = null;
        IAndroidDevice? androidDevice = null;
        IBrowserContext? context = null;
        PlaywrightBrowserSession? session = null;
        try
        {
            if (IsBrowserStackAndroid(options.BrowserStack))
            {
                androidDevice = await BrowserStackPlaywrightConnection.ConnectAndroidAsync(
                    playwright,
                    options.BrowserStack!);
                context = await androidDevice.LaunchBrowserAsync(new AndroidDeviceLaunchBrowserOptions
                {
                    BaseURL = options.BaseUrl.ToString()
                });
            }
            else
            {
                browser = options.BrowserStack is null
                    ? await LaunchLocalBrowserAsync(playwright, options)
                    : await BrowserStackPlaywrightConnection.ConnectBrowserAsync(playwright, options.BrowserStack);
                context = await browser.NewContextAsync(contextOptions);
            }
            await ApplyOriginScopedHeadersAsync(
                context,
                new[] { options.BaseUrl }.Concat(options.HeaderAllowedOrigins ?? []),
                options.OriginScopedHttpHeaders);
            if (UsesChromiumBrowser(options))
            {
                await GrantLocalNetworkAccessIfSupportedAsync(context, options.BaseUrl);
            }
            await VirginAtlanticConsent.ApplyEnsightenConsentAsync(context, options.BaseUrl);
            if (options.TraceEnabled)
            {
                await context.Tracing.StartAsync(new TracingStartOptions
                {
                    Screenshots = true,
                    Snapshots = true,
                    Sources = true
                });
            }

            IPage page = await context.NewPageAsync();
            page.SetDefaultTimeout(options.ActionTimeoutMs);
            page.SetDefaultNavigationTimeout(options.NavigationTimeoutMs);

            session = new(playwright, browser, androidDevice, context, page, options);
            session.CaptureNetworkDiagnostics(page);
            if (options.BrowserStack?.PreflightEnabled == true)
            {
                await session.CaptureBrowserStackEgressIpAsync();
            }

            await VirginAtlanticConsent.PrimeApplicationSessionAsync(
                page,
                options.BaseUrl,
                options.NavigationTimeoutMs);

            if (options.BrowserStack?.PreflightEnabled == true)
            {
                await session.AssertFrontendAccessibleAsync();
            }

            return session;
        }
        catch
        {
            if (session is not null)
            {
                await session.DisposeAsync();
            }
            else
            {
                await CleanupPartialStartAsync(playwright, browser, androidDevice, context);
            }
            throw;
        }
    }

    private static async Task CleanupPartialStartAsync(
        IPlaywright playwright,
        IBrowser? browser,
        IAndroidDevice? androidDevice,
        IBrowserContext? context)
    {
        await TryCleanupAsync(
            "partially started browser context",
            () => context?.CloseAsync().WaitAsync(TimeSpan.FromSeconds(20)) ?? Task.CompletedTask);
        await TryCleanupAsync(
            "partially started browser",
            () => browser?.CloseAsync().WaitAsync(TimeSpan.FromSeconds(20)) ?? Task.CompletedTask);
        await TryCleanupAsync(
            "partially started Android device",
            () => androidDevice?.CloseAsync().WaitAsync(TimeSpan.FromSeconds(20)) ?? Task.CompletedTask);
        TryCleanup("partially started Playwright driver", playwright.Dispose);
    }

    public async Task<string> SaveScreenshotAsync(string name)
    {
        string path = Path.Combine(_options.ArtifactDirectory, $"{SanitiseFileName(name)}.png");
        await Page.ScreenshotAsync(new PageScreenshotOptions
        {
            Path = path,
            FullPage = true
        });

        return path;
    }

    public async Task MarkPassedAsync(string reason)
    {
        _passed = true;
        try
        {
            await SetBrowserStackSessionStatusAsync(true, reason);
        }
        catch (Exception)
        {
            // Remote status publication is best effort and must not turn a successful test into a failure.
            Interlocked.Exchange(ref _browserStackStatusPublished, 1);
        }
    }

    public void SetCurrentAction(string action, string? failureContext = null)
    {
        if (!action.IsNullOrWhiteSpace())
        {
            _currentAction = action.Trim();
        }

        if (!failureContext.IsNullOrWhiteSpace())
        {
            _failureContext = failureContext!.Trim();
        }
    }

    public async Task<string> MarkFailedAsync(Exception exception)
    {
        try
        {
            await CaptureFailureScreenshotAsync();
            string? applicationError = await ReadVisibleApplicationErrorAsync();
            string failureReason = BuildFailureReason(exception, applicationError);
            Console.WriteLine($"[browserstack-failure] {failureReason}");
            await SetBrowserStackSessionStatusAsync(false, failureReason);
            return failureReason;
        }
        catch (Exception reportingException)
        {
            string fallbackReason =
                $"Unable to publish complete failure evidence while {_currentAction}. " +
                $"Original failure: {exception.GetType().Name}: {SanitizeDiagnosticText(exception.Message)}. " +
                $"Reporting failure: {reportingException.GetType().Name}: " +
                SanitizeDiagnosticText(reportingException.Message);
            Console.WriteLine($"[browserstack-failure-reporting] {fallbackReason}");
            return fallbackReason;
        }
    }

    public async Task SetBrowserStackSessionStatusAsync(bool passed, string reason)
    {
        if (_options.BrowserStack is null || Interlocked.CompareExchange(ref _browserStackStatusPublished, 1, 0) != 0)
        {
            return;
        }

        string payload = JsonSerializer.Serialize(new
        {
            action = "setSessionStatus",
            arguments = new
            {
                status = passed ? "passed" : "failed",
                reason
            }
        });

        try
        {
            await Page.EvaluateAsync<object>("_ => {}", $"browserstack_executor: {payload}")
                .WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch
        {
            // BrowserStack may close an otherwise successful session before accepting the status command.
            // Publishing status is observability only and must never alter the test outcome.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_options.BrowserStack is not null && Volatile.Read(ref _browserStackStatusPublished) == 0)
        {
            await TryCleanupAsync("BrowserStack failure screenshot", CaptureFailureScreenshotAsync);
            await TryCleanupAsync("BrowserStack terminal status", async () =>
            {
                string context = _failureContext.IsNullOrWhiteSpace()
                    ? string.Empty
                    : $"Scenario: {_failureContext}. ";
                await SetBrowserStackSessionStatusAsync(
                    false,
                    $"{context}Failed while {_currentAction}. The test ended before an exception-specific failure " +
                    $"could be published. URL: {SanitizeUrl(Page.Url)}");
            });
        }

        if (!_passed || _options.RetainPassedArtifacts)
        {
            await TryCleanupAsync(
                "network diagnostics",
                () => SaveNetworkDiagnosticsAsync().WaitAsync(TimeSpan.FromSeconds(10)));
        }

        if (_options.TraceEnabled)
        {
            string tracePath = Path.Combine(_options.ArtifactDirectory, "trace.zip");
            await TryCleanupAsync(
                "trace",
                () => Context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath })
                    .WaitAsync(TimeSpan.FromSeconds(20)));
        }

        await TryCleanupAsync(
            "browser context",
            () => Context.CloseAsync().WaitAsync(TimeSpan.FromSeconds(20)));
        await TryCleanupAsync(
            "browser",
            () => _browser?.CloseAsync().WaitAsync(TimeSpan.FromSeconds(20)) ?? Task.CompletedTask);
        await TryCleanupAsync(
            "Android device",
            () => _androidDevice?.CloseAsync().WaitAsync(TimeSpan.FromSeconds(20)) ?? Task.CompletedTask);
        TryCleanup("Playwright driver", _playwright.Dispose);

        if (_passed && !_options.RetainPassedArtifacts)
        {
            // StartAsync always replaces the configured artifact root with a unique
            // timestamp-and-GUID session directory before constructing this instance.
            TryCleanup("passed-session artifacts", () => Directory.Delete(_options.ArtifactDirectory, recursive: true));
        }
    }

    private static async Task TryCleanupAsync(string resource, Func<Task> cleanup)
    {
        try
        {
            await cleanup();
        }
        catch (Exception exception) when (IsBenignCleanupException(exception))
        {
            // Timeouts and already-closed transports are expected during best-effort cleanup.
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.TraceWarning(
                $"Failed to clean up {resource}: {exception}");
        }
    }

    private static void TryCleanup(string resource, Action cleanup)
    {
        try
        {
            cleanup();
        }
        catch (Exception exception) when (IsBenignCleanupException(exception))
        {
            // Timeouts and already-closed transports are expected during best-effort cleanup.
        }
        catch (Exception exception)
        {
            System.Diagnostics.Trace.TraceWarning(
                $"Failed to clean up {resource}: {exception}");
        }
    }

    private static bool IsBenignCleanupException(Exception exception) =>
        exception is TimeoutException
        || exception is PlaywrightException
            && exception.Message.Contains(
                "Target page, context or browser has been closed",
                StringComparison.OrdinalIgnoreCase);

    private string BuildFailureReason(Exception exception, string? applicationError)
    {
        string failureType = applicationError.IsNullOrWhiteSpace()
            ? "Automation/technical failure"
            : "Application failure";
        Exception innermostException = GetInnermostException(exception);
        string exceptionEvidence = innermostException == exception
            ? $"{exception.GetType().Name}: {exception.Message}"
            : $"{exception.GetType().Name}: {exception.Message} " +
              $"Inner failure: {innermostException.GetType().Name}: {innermostException.Message}";
        exceptionEvidence = SanitizeDiagnosticText(exceptionEvidence);
        string sanitizedApplicationError = SanitizeDiagnosticText(applicationError ?? string.Empty);
        string evidence = applicationError.IsNullOrWhiteSpace()
            ? exceptionEvidence
            : $"Visible error: {sanitizedApplicationError}. {exceptionEvidence}";
        string context = _failureContext.IsNullOrWhiteSpace() ? string.Empty : $"Scenario: {_failureContext}. ";
        string checkoutEvidence = BuildCheckoutDiagnosticSummary();
        string egressEvidence = _browserStackEgressIp.IsNullOrWhiteSpace()
            ? string.Empty
            : $"BrowserStack egress IP: {_browserStackEgressIp}. ";
        string reason =
            $"{context}{failureType} while {_currentAction}. {egressEvidence}{checkoutEvidence}{evidence} URL: {SanitizeUrl(Page.Url)}";
        return reason.Length <= 1_000 ? reason : reason[..1_000];
    }

    private async Task CaptureBrowserStackEgressIpAsync()
    {
        const string endpoint = "https://checkip.amazonaws.com/";
        string? error = null;

        IPage? egressPage = null;
        try
        {
            egressPage = await Context.NewPageAsync();
            await egressPage.GotoAsync(endpoint, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 20_000
            });
            string body = (await egressPage.Locator("body").InnerTextAsync()).Trim();
            if (!IPAddress.TryParse(body, out IPAddress? address))
            {
                throw new InvalidOperationException("The BrowserStack egress endpoint returned an invalid IP address.");
            }

            _browserStackEgressIp = address.ToString();
            Console.WriteLine(
                $"[browserstack-egress-ip] {_options.BrowserStack!.SessionName}: {_browserStackEgressIp}");
        }
        catch (Exception exception)
        {
            error = SanitizeDiagnosticText($"{exception.GetType().Name}: {exception.Message}");
            Console.WriteLine(
                $"[browserstack-egress-ip] Unable to capture the egress IP for " +
                $"{_options.BrowserStack!.SessionName}: {error}");
        }
        finally
        {
            if (egressPage is not null)
            {
                try
                {
                    await egressPage.CloseAsync();
                }
                catch (Exception exception) when (IsBenignCleanupException(exception))
                {
                    // The remote page can close with its BrowserStack transport.
                }
            }
        }

        await SaveBrowserStackPreflightAsync(null, error);
    }

    private async Task AssertFrontendAccessibleAsync()
    {
        try
        {
            await Page.WaitForFunctionAsync(
                @"() => {
                    const title = document.title || '';
                    const body = document.body?.innerText || '';
                    return /virgin\s+atlantic/i.test(`${title}\n${body}`);
                }",
                null,
                new PageWaitForFunctionOptions { Timeout = 20_000 });
            await SaveBrowserStackPreflightAsync(true, null);
            Console.WriteLine(
                $"[browserstack-preflight] Frontend accessible for {_options.BrowserStack!.SessionName}; " +
                $"egressIp={_browserStackEgressIp ?? "unknown"}; url={SanitizeUrl(Page.Url)}");
        }
        catch (Exception exception)
        {
            string error = $"{exception.GetType().Name}: {exception.Message}";
            await CaptureFailureScreenshotAsync();
            await SaveBrowserStackPreflightAsync(false, error);
            throw new InvalidOperationException(
                $"BrowserStack frontend preflight failed. URL: {SanitizeUrl(Page.Url)}. Title: {await Page.TitleAsync()}. " +
                $"Egress IP: {_browserStackEgressIp ?? "unknown"}. See browserstack-preflight.json and failure.png.",
                exception);
        }
    }

    private async Task SaveBrowserStackPreflightAsync(bool? frontendAccessible, string? error)
    {
        string path = Path.Combine(_options.ArtifactDirectory, "browserstack-preflight.json");
        string payload = JsonSerializer.Serialize(new
        {
            capturedAt = DateTimeOffset.UtcNow,
            session = _options.BrowserStack?.SessionName,
            browser = _options.BrowserStack?.Browser,
            operatingSystem = _options.BrowserStack?.OperatingSystem,
            egressIp = _browserStackEgressIp,
            baseUrl = _options.BaseUrl.GetLeftPart(UriPartial.Authority),
            currentUrl = SanitizeUrl(Page.Url),
            frontendAccessible,
            error = error is null ? null : SanitizeDiagnosticText(error)
        }, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(path, payload);
    }

    private string BuildCheckoutDiagnosticSummary()
    {
        NetworkDiagnostic[] checkoutDiagnostics = _networkDiagnostics
            .Where(diagnostic => diagnostic.IsCheckoutOrBasket)
            .TakeLast(4)
            .ToArray();
        if (checkoutDiagnostics.Length == 0)
        {
            return "Checkout/basket responses captured: none. ";
        }

        string summary = string.Join(" | ", checkoutDiagnostics.Select(diagnostic =>
        {
            string path = Uri.TryCreate(diagnostic.Url, UriKind.Absolute, out Uri? uri)
                ? uri.AbsolutePath
                : diagnostic.Url;
            string status = diagnostic.Status.HasValue
                ? diagnostic.Status.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)
                : diagnostic.Failure ?? "no-response";
            string basket = diagnostic.BasketId.IsNullOrWhiteSpace()
                ? string.Empty
                : $", basket={diagnostic.BasketId}";
            string correlation = diagnostic.CorrelationId.IsNullOrWhiteSpace()
                ? string.Empty
                : $", correlation={diagnostic.CorrelationId}";
            string request = diagnostic.RequestId.IsNullOrWhiteSpace()
                ? string.Empty
                : $", request={diagnostic.RequestId}";
            string error = diagnostic.ErrorSummary.IsNullOrWhiteSpace()
                ? string.Empty
                : $", error={diagnostic.ErrorSummary}";
            return $"{diagnostic.Method} {path} -> {status}{basket}{correlation}{request}{error}";
        }));

        return $"Latest checkout/basket responses: {summary}. ";
    }

    private static Exception GetInnermostException(Exception exception)
    {
        Exception innermostException = exception;
        while (innermostException.InnerException is not null)
        {
            innermostException = innermostException.InnerException;
        }

        return innermostException;
    }

    private async Task CaptureFailureScreenshotAsync()
    {
        try
        {
            await Page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(_options.ArtifactDirectory, "failure.png"),
                FullPage = true,
                Mask = _options.MaskSensitiveControlsInFailureScreenshots
                    ? new[] { Page.Locator("input, textarea, select, [contenteditable='true'], iframe") }
                    : null,
                MaskColor = "#000000"
            }).WaitAsync(TimeSpan.FromSeconds(10));
        }
        catch (Exception)
        {
            // The page may already be gone; trace and network diagnostics remain available.
        }
    }

    private async Task<string?> ReadVisibleApplicationErrorAsync()
    {
        try
        {
            return await Page.EvaluateAsync<string?>(
                @"() => {
                    const clean = (value) => (value || '').replace(/\s+/g, ' ').trim();
                    const visible = (element) => {
                        const rect = element.getBoundingClientRect();
                        const style = window.getComputedStyle(element);
                        return rect.width > 0 && rect.height > 0 &&
                            style.display !== 'none' && style.visibility !== 'hidden';
                    };
                    return [...document.querySelectorAll('[role=""alert""], [role=""dialog""], h1, h2, h3, p')]
                        .filter(visible)
                        .map((element) => clean(element.innerText || element.textContent))
                        .filter((text) => /Uh oh|something.?s not right|unable to book|encountered an error|payment.*failed/i.test(text))
                        .sort((left, right) => left.length - right.length)[0]
                        ?.slice(0, 500) || null;
                }").WaitAsync(TimeSpan.FromSeconds(5));
        }
        catch (Exception)
        {
            return null;
        }
    }

    private void CaptureNetworkDiagnostics(IPage page)
    {
        page.RequestFailed += (_, request) => _networkDiagnostics.Enqueue(new NetworkDiagnostic
        {
            Type = "requestfailed",
            Method = request.Method,
            Url = SanitizeUrl(request.Url),
            Failure = request.Failure,
            IsCheckoutOrBasket = IsCheckoutOrBasketUrl(request.Url)
        });
        page.Response += async (_, response) =>
        {
            try
            {
                bool isGraphQlResponse = response.Url.Contains("/api/graphql", StringComparison.OrdinalIgnoreCase);
                bool isCheckoutOrBasketResponse = IsCheckoutOrBasketUrl(response.Url);
                if (response.Status >= 400 || isGraphQlResponse || isCheckoutOrBasketResponse)
                {
                    string? responseBody = null;
                    if (response.Status >= 400 || isGraphQlResponse)
                    {
                        try
                        {
                            responseBody = await response.TextAsync();
                        }
                        catch (PlaywrightException)
                        {
                            // A response body can be unavailable when navigation closes the request context.
                        }
                    }

                    IReadOnlyDictionary<string, string>? requestHeaders = null;
                    IReadOnlyDictionary<string, string>? responseHeaders = null;
                    try
                    {
                        requestHeaders = await response.Request.AllHeadersAsync();
                        responseHeaders = await response.AllHeadersAsync();
                    }
                    catch (PlaywrightException)
                    {
                        // The context can close while a late response event is being processed.
                    }

                    string? basketId = FindHeader(requestHeaders, "basketid", "basket-id", "x-basket-id") ??
                        FindHeader(responseHeaders, "basketid", "basket-id", "x-basket-id");
                    IReadOnlyList<string>? requestHeaderNames = null;
                    IReadOnlyList<string>? cookieNames = null;
                    string? refererPath = null;
                    if (response.Status >= 400 || isGraphQlResponse)
                    {
                        try
                        {
                            requestHeaderNames = (requestHeaders ?? await response.Request.AllHeadersAsync()).Keys
                                .OrderBy(headerName => headerName, StringComparer.OrdinalIgnoreCase)
                                .ToArray();
                            if (isGraphQlResponse)
                            {
                                cookieNames = (await Context.CookiesAsync(new[] { response.Url }))
                                    .Select(cookie => cookie.Name)
                                    .OrderBy(cookieName => cookieName, StringComparer.OrdinalIgnoreCase)
                                    .ToArray();
                            }
                        }
                        catch (PlaywrightException)
                        {
                            // The context can close while a late response event is being processed.
                        }

                        if (response.Request.Headers.TryGetValue("referer", out string? referer) &&
                            Uri.TryCreate(referer, UriKind.Absolute, out Uri? refererUri))
                        {
                            refererPath = refererUri.AbsolutePath;
                        }
                    }

                    _networkDiagnostics.Enqueue(new NetworkDiagnostic
                    {
                        Type = isGraphQlResponse ? "graphql-response" : "response",
                        Method = response.Request.Method,
                        Url = SanitizeUrl(response.Url),
                        Status = response.Status,
                        StatusText = response.StatusText,
                        BasketId = basketId,
                        RequestHeaderNames = requestHeaderNames,
                        CookieNames = cookieNames,
                        GraphQlOperationName = GetGraphQlOperationName(response.Request.PostData),
                        RefererPath = refererPath,
                        IsCheckoutOrBasket = isCheckoutOrBasketResponse,
                        CorrelationId = FindHeader(
                            responseHeaders,
                            "x-correlation-id",
                            "correlation-id",
                            "traceparent"),
                        RequestId = FindHeader(
                            responseHeaders,
                            "x-request-id",
                            "request-id",
                            "apim-request-id"),
                        ErrorSummary = ExtractSafeApiErrorSummary(responseBody, response.Status >= 400)
                    });
                }
            }
            catch (Exception exception)
            {
                _networkDiagnostics.Enqueue(new NetworkDiagnostic
                {
                    Type = "diagnostic-error",
                    Method = response.Request.Method,
                    Url = SanitizeUrl(response.Url),
                    Status = response.Status,
                    Failure = exception.GetType().Name
                });
            }
        };
    }

    private static bool IsCheckoutOrBasketUrl(string url) =>
        url.Contains("/flight-only/api/checkout/", StringComparison.OrdinalIgnoreCase) ||
        url.Contains("/api/checkout/", StringComparison.OrdinalIgnoreCase) ||
        url.Contains("/basket", StringComparison.OrdinalIgnoreCase);

    private static string SanitizeUrl(string url) => DiagnosticSanitizer.SanitizeUrl(url);

    private static string SanitizeDiagnosticText(string text) => DiagnosticSanitizer.Sanitize(text);

    private static async Task ApplyOriginScopedHeadersAsync(
        IBrowserContext context,
        IEnumerable<Uri> allowedOrigins,
        IReadOnlyDictionary<string, string>? headers)
    {
        if (headers is null || headers.Count == 0)
        {
            return;
        }

        HashSet<string> allowedAuthorities = allowedOrigins
            .Select(origin => origin.GetLeftPart(UriPartial.Authority))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        await context.RouteAsync("**/*", async route =>
        {
            if (!Uri.TryCreate(route.Request.Url, UriKind.Absolute, out Uri? requestUri) ||
                !allowedAuthorities.Contains(requestUri.GetLeftPart(UriPartial.Authority)))
            {
                await route.ContinueAsync();
                return;
            }

            Dictionary<string, string> scopedHeaders = new(
                route.Request.Headers,
                StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, string> header in headers)
            {
                scopedHeaders[header.Key] = header.Value;
            }

            await route.ContinueAsync(new RouteContinueOptions { Headers = scopedHeaders });
        });
    }

    private async Task SaveNetworkDiagnosticsAsync()
    {
        string path = Path.Combine(_options.ArtifactDirectory, "network-diagnostics.json");
        await File.WriteAllTextAsync(path, JsonSerializer.Serialize(_networkDiagnostics, new JsonSerializerOptions
        {
            WriteIndented = true
        }));
    }

    private static string? GetGraphQlOperationName(string? postData)
    {
        if (postData.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(postData);
            return document.RootElement.TryGetProperty("operationName", out JsonElement operationName)
                ? operationName.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? FindHeader(
        IReadOnlyDictionary<string, string>? headers,
        params string[] candidateNames)
    {
        if (headers is null)
        {
            return null;
        }

        foreach (string candidateName in candidateNames)
        {
            KeyValuePair<string, string>? match = headers.FirstOrDefault(header =>
                header.Key.Equals(candidateName, StringComparison.OrdinalIgnoreCase));
            if (match is not null && !StringExtensions.IsNullOrWhiteSpace(match.Value.Value))
            {
                return match.Value.Value;
            }
        }

        return null;
    }

    private static string? ExtractSafeApiErrorSummary(string? responseBody, bool includeRootFields)
    {
        if (responseBody.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(responseBody);
            List<string> fields = [];
            CollectSafeErrorFields(document.RootElement, includeRootFields, fields);
            return fields.Count == 0 ? null : string.Join("; ", fields.Distinct().Take(20));
        }
        catch (JsonException)
        {
            return includeRootFields ? "Non-JSON API error response" : null;
        }
    }

    private static void CollectSafeErrorFields(
        JsonElement element,
        bool withinError,
        ICollection<string> fields)
    {
        if (fields.Count >= 20)
        {
            return;
        }

        if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in element.EnumerateArray())
            {
                CollectSafeErrorFields(item, withinError, fields);
            }

            return;
        }

        if (element.ValueKind != JsonValueKind.Object)
        {
            return;
        }

        foreach (JsonProperty property in element.EnumerateObject())
        {
            bool propertyIsError = property.Name.Contains("error", StringComparison.OrdinalIgnoreCase);
            bool childWithinError = withinError || propertyIsError;
            bool safeField = childWithinError && IsSafeErrorField(property.Name);
            if (safeField && property.Value.ValueKind is JsonValueKind.String or JsonValueKind.Number)
            {
                string value = SanitizeDiagnosticText(
                    property.Value.ToString().ReplaceLineEndings(" ").Trim());
                if (!value.IsNullOrWhiteSpace())
                {
                    fields.Add($"{property.Name}={value[..Math.Min(value.Length, 300)]}");
                }
            }

            CollectSafeErrorFields(property.Value, childWithinError, fields);
        }
    }

    private static bool IsSafeErrorField(string propertyName) =>
        propertyName.Equals("code", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("errorCode", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("message", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("reason", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("status", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("title", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("correlationId", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("requestId", StringComparison.OrdinalIgnoreCase) ||
        propertyName.Equals("traceId", StringComparison.OrdinalIgnoreCase);

    private static async Task GrantLocalNetworkAccessIfSupportedAsync(IBrowserContext context, Uri baseUrl)
    {
        try
        {
            string origin = baseUrl.GetLeftPart(UriPartial.Authority);
            await context.GrantPermissionsAsync(
                new[] { "local-network-access" },
                new BrowserContextGrantPermissionsOptions { Origin = origin });
        }
        catch (PlaywrightException)
        {
            // Older browser engines do not recognise Chromium local network access permissions.
        }
    }

    private static async Task<IBrowser> LaunchLocalBrowserAsync(
        IPlaywright playwright,
        PlaywrightBrowserOptions options)
    {
        List<string> browserArguments = new()
        {
            "--disable-features=LocalNetworkAccessChecks,LocalNetworkAccessPermissionPrompt"
        };
        if (options.DisableWebSecurity && UsesChromiumBrowser(options))
        {
            browserArguments.Add("--disable-web-security");
            browserArguments.Add("--disable-site-isolation-trials");
        }

        BrowserTypeLaunchOptions launchOptions = new()
        {
            Headless = options.Headless,
            SlowMo = options.SlowMoMs,
            Channel = options.BrowserChannel,
            Args = browserArguments
        };

        string normalisedBrowserName = options.BrowserName.Trim().ToLowerInvariant();
        return normalisedBrowserName switch
        {
            "firefox" => await playwright.Firefox.LaunchAsync(launchOptions),
            "webkit" => await playwright.Webkit.LaunchAsync(launchOptions),
            _ => await playwright.Chromium.LaunchAsync(launchOptions)
        };
    }

    private static bool IsBrowserStackAndroid(BrowserStackPlaywrightOptions? options) =>
        options?.DeviceName is not null && options.Browser.Is("chrome");

    private static bool UsesChromiumBrowser(PlaywrightBrowserOptions options) =>
        options.BrowserStack is null
            ? options.BrowserName.Is("chromium")
            : options.BrowserStack.Browser.Is("chrome") || options.BrowserStack.Browser.Is("edge");

    private static string SanitiseFileName(string value)
    {
        char[] invalidCharacters = Path.GetInvalidFileNameChars();
        string sanitised = string.Join("_", value.Split(invalidCharacters, StringSplitOptions.RemoveEmptyEntries));
        return sanitised.IsNullOrWhiteSpace() ? "screenshot" : sanitised;
    }
}

internal sealed class NetworkDiagnostic
{
    public string Type { get; init; } = string.Empty;

    public string Method { get; init; } = string.Empty;

    public string Url { get; init; } = string.Empty;

    public int? Status { get; init; }

    public string? StatusText { get; init; }

    public string? Failure { get; init; }

    public bool IsCheckoutOrBasket { get; init; }

    public string? BasketId { get; init; }

    public IReadOnlyList<string>? RequestHeaderNames { get; init; }

    public IReadOnlyList<string>? CookieNames { get; init; }

    public string? GraphQlOperationName { get; init; }

    public string? RefererPath { get; init; }

    public string? CorrelationId { get; init; }

    public string? RequestId { get; init; }

    public string? ErrorSummary { get; init; }
}
