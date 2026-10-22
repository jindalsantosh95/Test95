using OpenQA.Selenium;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

// Only a completed readiness wait with no observed destination permits another Account tap.
internal sealed class AccountEntryReadinessTimeoutException : WebDriverTimeoutException
{
    public AccountEntryReadinessTimeoutException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
