using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom.ElementLibrary;

namespace VAA.EAPI.Automation.Heimdall.Tests.HealthChecks.DotCom.PageObjects
{
    public class LoyaltyLogin
    {
        private readonly IWebDriver _webDriver;

        public LoyaltyLogin(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public void Login(string username, string password)
        {
            _webDriver.WaitForElementsToBeVisible(LoyaltyLoginPageElements.CookieAgree);

            _webDriver.FindElementAndClick(LoyaltyLoginPageElements.CookieAgree, "Agree to Cookies");

            _webDriver.WaitForElementsToBeVisible(LoyaltyLoginPageElements.EmailOrFlyingClubNumber);

            _webDriver.FindElementAndEnterText(LoyaltyLoginPageElements.EmailOrFlyingClubNumber, username, "Enter Email or flying club number");

            _webDriver.WaitForElementsToBeVisible(LoyaltyLoginPageElements.Password);

            _webDriver.FindElementAndEnterText(LoyaltyLoginPageElements.Password, password, "Enter password");

            _webDriver.WaitForElementsToBeVisible(LoyaltyLoginPageElements.Continue);

            _webDriver.FindElementAndClick(LoyaltyLoginPageElements.Continue, "Click Continue");

            _webDriver.WaitForElementsToBeVisible(LoyaltyLoginPageElements.SendButton);

            _webDriver.FindElementAndClick(LoyaltyLoginPageElements.SendButton, "Send Button");
        }

        public void VerifyCode(string code)
        {
            _webDriver.WaitForElementsToBeVisible(LoyaltyLoginPageElements.VerificationCode);

            _webDriver.FindElementAndEnterText(LoyaltyLoginPageElements.VerificationCode, code, "Enter code");

            _webDriver.WaitForElementsToBeVisible(LoyaltyLoginPageElements.VerifyCodeButton);

            _webDriver.FindElementAndClick(LoyaltyLoginPageElements.VerifyCodeButton, "Click verify code");
        }

        public void WaitForAccountOverview()
        {
            _webDriver.WaitForRedirectUrl("account/overview", 20);
        }
    }
}
