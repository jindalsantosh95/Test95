using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.Playwright.Browser;

public sealed record PlaywrightBrowserOptions
{
    public Uri BaseUrl { get; init; } = new("https://example.invalid/");

    public string BrowserName { get; init; } = "chromium";

    public string? BrowserChannel { get; init; }

    // Applied only to requests matching BaseUrl's scheme, host and port.
    public IReadOnlyDictionary<string, string>? OriginScopedHttpHeaders { get; init; }

    // Additional exact origins which may receive OriginScopedHttpHeaders. BaseUrl is always included.
    public IReadOnlyCollection<Uri>? HeaderAllowedOrigins { get; init; }

    public bool DisableWebSecurity { get; init; }

    public bool Headless { get; init; } = true;

    public int SlowMoMs { get; init; }

    public int DefaultTimeoutMs { get; init; } = 30_000;

    public int ActionTimeoutMs { get; init; } = 30_000;

    public int AssertionTimeoutMs { get; init; } = 30_000;

    public int NavigationTimeoutMs { get; init; } = 60_000;

    public bool IsContinuousIntegration { get; init; }

    public BrowserStackPlaywrightOptions? BrowserStack { get; init; }

    public bool IsBrowserStack => BrowserStack is not null;

    public int ViewportWidth { get; init; } = 1440;

    public int ViewportHeight { get; init; } = 1000;

    public bool TraceEnabled { get; init; } = true;

    public bool RetainPassedArtifacts { get; init; } = true;

    public bool MaskSensitiveControlsInFailureScreenshots { get; init; }

    public string ArtifactDirectory { get; init; } = ResolveArtifactDirectory(Path.Combine("TestResults", "playwright"));

    public static PlaywrightBrowserOptions FromEnvironment(string baseUrl)
    {
        bool useBrowserStack = ReadString("SAB_RUN_MODE", "Local").Is("BrowserStack");
        string browserName = ReadString("SAB_BROWSER", "chromium");

        return new PlaywrightBrowserOptions
        {
            BaseUrl = new Uri(baseUrl, UriKind.Absolute),
            BrowserName = browserName,
            BrowserChannel = browserName.Is("chromium") ? ReadOptionalString("SAB_BROWSER_CHANNEL") : null,
            DisableWebSecurity = ReadBool("SAB_DISABLE_WEB_SECURITY", false),
            Headless = ReadBool("SAB_HEADLESS", true),
            SlowMoMs = ReadInt("SAB_SLOW_MO_MS", 0),
            DefaultTimeoutMs = ReadInt("SAB_DEFAULT_TIMEOUT_MS", 30_000),
            ActionTimeoutMs = ReadInt("SAB_ACTION_TIMEOUT_MS", 30_000),
            AssertionTimeoutMs = ReadInt("SAB_ASSERTION_TIMEOUT_MS", 30_000),
            NavigationTimeoutMs = ReadInt("SAB_NAVIGATION_TIMEOUT_MS", 60_000),
            ViewportWidth = ReadInt("SAB_VIEWPORT_WIDTH", 1440),
            ViewportHeight = ReadInt("SAB_VIEWPORT_HEIGHT", 1000),
            TraceEnabled = ReadBool("SAB_TRACE", !useBrowserStack),
            IsContinuousIntegration = IsCiEnvironment(),
            BrowserStack = useBrowserStack ? BrowserStackPlaywrightOptions.FromEnvironment() : null,
            ArtifactDirectory = ResolveArtifactDirectory(ReadString("SAB_ARTIFACT_DIR", Path.Combine("TestResults", "playwright")))
        };
    }

    private static string ResolveArtifactDirectory(string value)
    {
        if (Path.IsPathRooted(value))
        {
            return value;
        }

        return Path.GetFullPath(Path.Combine(FindSolutionRoot(), value));
    }

    private static string FindSolutionRoot()
    {
        return FindSolutionRootFrom(Environment.CurrentDirectory) ??
            FindSolutionRootFrom(AppContext.BaseDirectory) ??
            Environment.CurrentDirectory;
    }

    private static string? FindSolutionRootFrom(string startPath)
    {
        DirectoryInfo? directory = new(startPath);
        while (directory is not null)
        {
            string solutionPath = Path.Combine(directory.FullName, "VAA.EAPI.Automation.sln");
            if (File.Exists(solutionPath))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    private static string ReadString(string name, string defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return value.IsNullOrWhiteSpace() ? defaultValue : value!;
    }

    private static string? ReadOptionalString(string name)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return value.IsNullOrWhiteSpace() ? null : value;
    }

    private static bool ReadBool(string name, bool defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return bool.TryParse(value, out bool parsed) ? parsed : defaultValue;
    }

    private static int ReadInt(string name, int defaultValue)
    {
        string? value = Environment.GetEnvironmentVariable(name);
        return int.TryParse(value, out int parsed) ? parsed : defaultValue;
    }

    private static bool IsCiEnvironment()
    {
        return ReadBool("CI", false) ||
            ReadBool("GITHUB_ACTIONS", false) ||
            ReadBool("TF_BUILD", false);
    }
}
