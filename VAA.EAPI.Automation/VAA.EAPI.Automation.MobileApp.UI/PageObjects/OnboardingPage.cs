using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

/// <summary>Handles the Airship onboarding carousel: Get started -> Continue -> Maybe later.</summary>
public sealed class OnboardingPage : NativeAppPage
{
    public OnboardingPage(AppiumDriver driver) : base(driver, TimeSpan.FromSeconds(30)) { }

    public bool CompleteIfPresent()
    {
        IWebElement? getStarted = TryWaitForElementByKey(WelcomePage.GetStarted, TimeSpan.FromMilliseconds(500));
        if (getStarted is null)
        {
            return false;
        }

        Complete(getStarted);
        return true;
    }

    public void Complete(IWebElement getStarted)
    {
        getStarted.Click();
        WaitForCarouselButton("Continue").Click();
        WaitForCarouselButton("Maybe later").Click();
    }

    private IWebElement WaitForCarouselButton(string accessibilityId)
        => Wait.Until(_ => FindButtonInViewport(accessibilityId));

    private IWebElement? FindButtonInViewport(string accessibilityId)
    {
        int viewportWidth = Driver.Manage().Window.Size.Width;
        foreach (IWebElement button in Driver.FindElements(MobileBy.AccessibilityId(accessibilityId)))
        {
            try
            {
                if (button.Displayed && button.Enabled && button.Location.X >= 0 && button.Location.X < viewportWidth)
                {
                    return button;
                }
            }
            catch (StaleElementReferenceException)
            {
                // Airship rebuilds its element tree between carousel pages.
            }
        }
        return null;
    }
}
