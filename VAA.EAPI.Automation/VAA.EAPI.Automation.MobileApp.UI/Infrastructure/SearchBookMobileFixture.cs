using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.UI.Infrastructure;

namespace VAA.EAPI.Automation.MobileApp.UI.Infrastructure;

public sealed class SearchBookMobileFixture : IAsyncLifetime
{
    private readonly MobileFixture _mobileFixture;

    public SearchBookMobileFixture(
        IReadOnlyDictionary<string, string?>? configurationOverrides = null)
    {
        _mobileFixture = new MobileFixture(configurationOverrides);
    }

    public AppiumDriver Driver => _mobileFixture.Driver;

    public IConfiguration Config => _mobileFixture.Config;

    public async Task InitializeAsync()
    {
        await _mobileFixture.InitializeAsync();
    }

    public Task DisposeAsync() => _mobileFixture.DisposeAsync();
}
