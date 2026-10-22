using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Support.UI;
using VAA.EAPI.Automation.Core.TestDataConfig.APMPayment;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>External-provider DOM contract. Submits sandbox approval once, never retries payment.</summary>
public sealed class NativePayPalPage(AppiumDriver driver)
{
    public void SignInAndApprove(APMDetails account, Action<string>? reportProgress = null)
    {
        string stage = "opening PayPal";
        try
        {
            WebDriverWait wait = new(driver, TimeSpan.FromSeconds(45));
            wait.Until(_ => driver.FindElements(MobileBy.AccessibilityId("PayPal"))
                .SingleOrDefault(element => element.Displayed && element.Enabled))!.Click();
            IContextAware contexts = (IContextAware)driver;
            // The web suite owns these provider selectors. Locate the actual HPP button,
            // not an arbitrary first WebView. Missing inspectable content fails explicitly.
            wait.Until(_ => FindPayPalButton(contexts))!.Click();
            stage = "finding the sandbox login";
            reportProgress?.Invoke("PayPal launch requested; locating sandbox login.");
            wait.Until(_ => SwitchToSandbox(contexts));
            stage = "entering sandbox email";
            reportProgress?.Invoke("PayPal sandbox login located.");
            IWebElement email = Visible(wait, By.CssSelector("input#email"));
            RequireSandbox();
            email.SendKeys(account.Email);
            RequireSandbox();
            Visible(wait, By.CssSelector("button[value='Next']")).Click();
            stage = "entering sandbox password";
            IWebElement password = Visible(wait, By.CssSelector("input#password"));
            RequireSandbox();
            password.SendKeys(account.Password);
            RequireSandbox();
            Visible(wait, By.CssSelector("button[value='Login']")).Click();
            stage = "waiting for authenticated approval readiness";
            reportProgress?.Invoke("PayPal login submitted once; waiting for approval control.");
            IWebElement approve = Visible(wait, By.CssSelector("button[data-id='payment-submit-btn']"));
            RequireSandbox();
            stage = "submitting sandbox approval once";
            approve.Click();
            reportProgress?.Invoke("PayPal approval clicked once; awaiting app confirmation.");
        }
        catch (Exception)
        {
            reportProgress?.Invoke($"PayPal stopped while {stage}; provider details suppressed.");
            // WebDriver errors may include URLs or field contents. Never retain the raw exception.
            throw new InvalidOperationException($"PayPal failed while {stage}. No approval or login retry; sensitive diagnostics omitted.");
        }
        finally
        {
            try
            { ((IContextAware)driver).Context = "NATIVE_APP"; }
            catch (WebDriverException) { /* Session teardown owns cleanup; never leak provider diagnostics. */ }
        }
    }

    internal static bool IsSandboxUrl(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) && uri.Scheme == "https" &&
            (uri.Host == "sandbox.paypal.com" || uri.Host.EndsWith(".sandbox.paypal.com", StringComparison.Ordinal)) &&
            string.IsNullOrEmpty(uri.UserInfo);

    private void RequireSandbox()
    {
        if (!IsSandboxUrl(driver.Url))
            throw new InvalidOperationException("PayPal is not on an approved sandbox origin.");
    }

    private IWebElement Visible(WebDriverWait wait, By locator)
        => wait.Until(_ => driver.FindElements(locator).SingleOrDefault(element => element.Displayed && element.Enabled))!;

    private IWebElement? FindPayPalButton(IContextAware contexts)
    {
        foreach (string context in contexts.Contexts.Where(value => value.StartsWith("WEBVIEW", StringComparison.Ordinal)))
        {
            contexts.Context = context;
            foreach (string window in driver.WindowHandles)
            {
                driver.SwitchTo().Window(window);
                IWebElement? button = driver.FindElements(By.CssSelector("div[id*='paypal-buttons']"))
                    .SingleOrDefault(element => element.Displayed && element.Enabled);
                if (button is not null)
                    return button;
            }
        }
        return null;
    }

    private bool SwitchToSandbox(IContextAware contexts)
    {
        foreach (string context in contexts.Contexts.Where(value => value.StartsWith("WEBVIEW", StringComparison.Ordinal)))
        {
            contexts.Context = context;
            foreach (string window in driver.WindowHandles)
            {
                driver.SwitchTo().Window(window);
                if (IsSandboxUrl(driver.Url))
                    return true;
            }
        }
        return false;
    }
}
