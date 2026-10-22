using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.MobileApp.UI.Models;

namespace VAA.EAPI.Automation.MobileApp.UI.PageObjects;

public sealed class LoyaltyPersonalDetailsPage : NativeAppPage
{
    public const string SettingsButton = "settingsBarButton";
    public const string SettingsScreen = "yourAccountLabel";
    public const string PersonalDetailsRow = "personalDetailsRow";
    public const string MemberNameHeader = "title";
    public const string EditDetails = "guestDiscoverFCButton";
    public const string DeleteAccount = "deleteAccountButton";

    private static readonly TimeSpan ScreenTimeout = TimeSpan.FromSeconds(30);

    public LoyaltyPersonalDetailsPage(AppiumDriver driver)
        : base(driver, ScreenTimeout)
    {
    }

    public void Open()
    {
        TapByKey(SettingsButton);
        WaitForElementByKey(SettingsScreen);
        TapByKey(PersonalDetailsRow);
        WaitForElementByKey(MemberNameHeader);
    }

    public LoyaltyPersonalDetails ReadDetails()
    {
        string memberName = ReadText(WaitForElementByKey(MemberNameHeader));
        bool editDetailsAvailable = FindWithBoundedScroll(EditDetails, maximumScrolls: 8) is not null;
        bool deleteAccountAvailable = FindWithBoundedScroll(DeleteAccount, maximumScrolls: 4) is not null;

        return new LoyaltyPersonalDetails(
            memberName,
            editDetailsAvailable,
            deleteAccountAvailable);
    }

    public void ReturnToAccount()
    {
        Driver.Navigate().Back();
        WaitForElementByKey(SettingsScreen);
        Driver.Navigate().Back();
        WaitForElementByKey(SettingsButton);
    }

    private IWebElement? FindWithBoundedScroll(string accessibilityId, int maximumScrolls)
    {
        for (int attempt = 0; attempt <= maximumScrolls; attempt++)
        {
            IWebElement? element = Driver.FindElements(MobileBy.AccessibilityId(accessibilityId))
                .FirstOrDefault(IsDisplayed);
            if (element is not null)
            {
                return element;
            }

            if (attempt < maximumScrolls)
            {
                Driver.ExecuteScript("mobile: scroll", new Dictionary<string, object>
                {
                    ["direction"] = "down",
                });
            }
        }

        return null;
    }

    private static bool IsDisplayed(IWebElement element)
    {
        try
        {
            return element.Displayed;
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    private static string ReadText(IWebElement element)
    {
        string text = element.Text.Trim();
        if (!text.IsNullOrWhiteSpace())
        {
            return text;
        }

        string label = element.GetAttribute("label")?.Trim() ?? string.Empty;
        return !label.IsNullOrWhiteSpace()
            ? label
            : element.GetAttribute("value")?.Trim() ?? string.Empty;
    }
}
