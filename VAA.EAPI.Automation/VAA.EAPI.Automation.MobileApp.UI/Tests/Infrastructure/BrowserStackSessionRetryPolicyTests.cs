using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Factory;

namespace VAA.EAPI.Automation.MobileApp.UI.Tests.Infrastructure;

public sealed class BrowserStackSessionRetryPolicyTests
{
    [Theory]
    [InlineData("BROWSERSTACK_SO_TIMEOUT")]
    [InlineData("BROWSERSTACK_COULD_NOT_LAUNCH_APP")]
    [InlineData("503 Service Unavailable")]
    [InlineData("The connection was closed by the remote host")]
    public void IsTransient_InfrastructureFailure_ReturnsTrue(string message)
    {
        WebDriverException exception = new(message);

        bool result = BrowserStackSessionRetryPolicy.IsTransient(exception);

        Assert.True(result);
    }

    [Theory]
    [InlineData("BROWSERSTACK_INVALID_APP_CAP")]
    [InlineData("BROWSERSTACK_INVALID_DEVICE")]
    [InlineData("Authentication failed")]
    [InlineData("Payment-reference creation failed before HPP opened")]
    [InlineData("Secure payment failed after HPP submission")]
    public void IsTransient_ConfigurationOrProductFailure_ReturnsFalse(string message)
    {
        WebDriverException exception = new(message);

        bool result = BrowserStackSessionRetryPolicy.IsTransient(exception);

        Assert.False(result);
    }
}
