using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.SearchAndBook.UI.Tests.Setup;

public sealed class PlaywrightSetupVerificationTests
{
    private readonly ITestOutputHelper _output;

    public PlaywrightSetupVerificationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    [Trait("Category", "SearchAndBook-UI-Setup")]
    [Trait("tag", "search-and-book-ui-setup")]
    public async Task Setup_TC01_LocalBrowser_RendersAndHandlesInteraction()
    {
        string artifactDirectory = Environment.GetEnvironmentVariable("SAB_ARTIFACT_DIR") ??
            Path.GetFullPath(Path.Combine("TestResults", "playwright-setup"));
        string browserName = Environment.GetEnvironmentVariable("SAB_BROWSER") ?? "chromium";
        Directory.CreateDirectory(artifactDirectory);

        using IPlaywright playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        IBrowserType browserType = browserName.ToLowerInvariant() switch
        {
            "firefox" => playwright.Firefox,
            "webkit" => playwright.Webkit,
            _ => playwright.Chromium
        };
        await using IBrowser browser = await browserType.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
        await using IBrowserContext context = await browser.NewContextAsync();
        await context.Tracing.StartAsync(new TracingStartOptions
        {
            Screenshots = true,
            Snapshots = true,
            Sources = true
        });

        IPage page = await context.NewPageAsync();
        await page.SetContentAsync(
            """
            <!doctype html>
            <html lang="en">
              <head><title>Search &amp; Book setup verification</title></head>
              <body>
                <main>
                  <h1>Playwright is ready</h1>
                  <button id="verify" type="button" onclick="this.textContent='Interaction verified'">Verify interaction</button>
                </main>
              </body>
            </html>
            """);

        await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Verify interaction" }).ClickAsync();
        string buttonText = await page.Locator("#verify").InnerTextAsync();
        string title = await page.TitleAsync();
        string screenshotPath = Path.Combine(artifactDirectory, "setup-verification.png");
        string tracePath = Path.Combine(artifactDirectory, "setup-trace.zip");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
        await context.Tracing.StopAsync(new TracingStopOptions { Path = tracePath });

        _output.WriteLine($"Browser: {browserName}");
        _output.WriteLine($"Screenshot: {screenshotPath}");
        _output.WriteLine($"Trace: {tracePath}");

        Assert.Equal("Search & Book setup verification", title);
        Assert.Equal("Interaction verified", buttonText);
        Assert.True(File.Exists(screenshotPath), $"Expected setup screenshot at {screenshotPath}.");
        Assert.True(File.Exists(tracePath), $"Expected setup trace at {tracePath}.");
    }
}
