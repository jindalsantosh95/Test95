using VAA.EAPI.Automation.Core.Playwright.Browser;
using VAA.EAPI.Automation.InFlightApplications.UI.Assertions;
using VAA.EAPI.Automation.InFlightApplications.UI.Flows;

namespace VAA.EAPI.Automation.InFlightApplications.UI.Tests.Smoke;

public sealed class EntryPointTests
{
    [SkippableTheory]
    [Trait("Category", "InFlightApplications-UI-Smoke")]
    [InlineData("/", "/")]
    [InlineData("/starlink-captive-portal/", "/starlink-captive-portal")]
    public async Task EntryPoint_Loads(string relativePath, string expectedPath)
    {
        PlaywrightBrowserOptions options = TestOptions.FromEnvironment();
        PortalValidationFlow flow = new(options);

        EntryPointResult result = await flow.LoadEntryPointAsync(relativePath);

        PortalAssertions.EntryPointLoaded(result, expectedPath);
    }
}
