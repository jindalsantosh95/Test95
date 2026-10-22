using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.Core.UI.Infrastructure;
using VAA.EAPI.Automation.MobileApp.UI.PageObjects;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Connectivity;

[Collection("Mobile")]
public sealed class MobileAppConnectivityTests : IAsyncLifetime
{
    private readonly MobileFixture _fixture = new();
    private readonly ITestOutputHelper _output;

    public MobileAppConnectivityTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public Task InitializeAsync() => _fixture.InitializeAsync();

    public Task DisposeAsync() => _fixture.DisposeAsync();

    [Fact]
    [Trait("Category", "MobileApp-Connectivity")]
    [Trait("TestType", "Connectivity")]
    public void MobileApp_Connectivity_AppiumSessionStartsSuccessfully()
    {
        try
        {
            AppiumDriver driver = _fixture.Driver;
            string sessionId = driver.SessionId?.ToString() ?? string.Empty;

            Assert.False(sessionId.IsNullOrWhiteSpace(), "Appium did not return a session ID.");
            string entryPoint = new WelcomePage(driver).WaitForRecognisedEntryPoint();
            Assert.False(entryPoint.IsNullOrWhiteSpace(), "The app did not reach a recognised ready entry point.");

            TrySetSessionStatus(
                "passed",
                $"Connectivity fixture created an Appium session and reached landing entry point '{entryPoint}'.");
        }
        catch (Exception exception)
        {
            TrySetSessionStatus("failed", exception.Message);
            throw;
        }
    }

    private void TrySetSessionStatus(string status, string reason)
    {
        try
        {
            BrowserStackStatusHelper.SetSessionStatus(_fixture.Driver, status, reason);
        }
        catch (Exception exception)
        {
            _output.WriteLine($"BrowserStack status update was unavailable: {exception.Message}");
        }
    }
}
