using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MyAccount.UI.PageObjects
{
    public class WifiSignInPage
    {
        private readonly IWebDriver webDriver;

        public WifiSignInPage(IWebDriver driver)
        {
            webDriver = driver;
        }

        public void ClickYesIAgree(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiSignIn.yesIAgreeButton, "Yes, I Agree button");
            logger?.Log(Status.Info, "Yes, I Agree button");
        }
        public void ClickLogIn(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiSignIn.loginButton, "Log In button");
            logger?.Log(Status.Info, "Clicked on Log In button");
        }

        public void ClickJoinNow(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiSignIn.joinNowButton, "Join now button");
            logger?.Log(Status.Info, "Clicked on Join now button");
        }

        public void ClickPasswordReset(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiSignIn.forgotPasswordLink, "Password Reset link");
            logger?.Log(Status.Info, "Clicked on Password Reset link");
        }

        public void EnterEmailOrLoyaltyAccountNumber(ExtentTest logger, string emailOrLoyaltyAccountNumber)
        {
            webDriver.FindElementAndEnterText(WifiSignIn.emailOrLoyaltyAccountNumberField, emailOrLoyaltyAccountNumber, "Email or Loyalty Account Number field");
            logger?.Log(Status.Info, $"Entered : {emailOrLoyaltyAccountNumber}");
        }

        public void EnterPassword(ExtentTest logger, string password)
        {
            webDriver.FindElementAndEnterText(WifiSignIn.passwordField, password, "Password field");
            logger?.Log(Status.Info, $"Entered email: {password}");
        }

        public string GetConnectedMessage(ExtentTest logger)
        {
            webDriver.WaitForElementToBeVisible(WifiSignIn.connectedMessage);
            logger?.Log(Status.Info, "Connected message displayed");
            return webDriver.GetElementText(WifiSignIn.connectedMessage);
        }

        public void TabOutOfEmail(ExtentTest logger)
        {
            webDriver.FindElement(WifiSignIn.emailOrLoyaltyAccountNumberField, "Email or Loyalty Account Number field").SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Tabbed out of email field");
        }

        public void ClearEmail(ExtentTest logger)
        {
            var element = webDriver.FindElement(WifiSignIn.emailOrLoyaltyAccountNumberField, "Email or Loyalty Account Number field");
            element.Click();
            element.SendKeys("a");
            element.SendKeys(Keys.Control + "a");
            element.SendKeys(Keys.Backspace);
            element.SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Cleared email field and tabbed away");
        }

        public void ClearPassword(ExtentTest logger)
        {
            var element = webDriver.FindElement(WifiSignIn.passwordField, "Password field");
            element.Click();
            element.SendKeys("a");
            element.SendKeys(Keys.Control + "a");
            element.SendKeys(Keys.Backspace);
            element.SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Cleared password field and tabbed away");
        }

        public string GetEmailValidationError(ExtentTest logger)
        {
            try
            {
                webDriver.WaitForElementToBeVisible(WifiSignIn.emailRequiredError, 5);
                var text = webDriver.GetElementText(WifiSignIn.emailRequiredError);
                logger?.Log(Status.Info, $"Email required validation: {text}");
                return text;
            }
            catch (Exception ex) when (ex is WebDriverTimeoutException or NoSuchElementException)
            {
                webDriver.WaitForElementToBeVisible(WifiSignIn.emailInvalidError, 5);
                var text = webDriver.GetElementText(WifiSignIn.emailInvalidError);
                logger?.Log(Status.Info, $"Email invalid validation: {text}");
                return text;
            }
        }

        public string GetPasswordValidationError(ExtentTest logger)
        {
            webDriver.WaitForElementToBeVisible(WifiSignIn.passwordRequiredError, 5);
            var text = webDriver.GetElementText(WifiSignIn.passwordRequiredError);
            logger?.Log(Status.Info, $"Password validation: {text}");
            return text;
        }

        public string GetAccountNotFoundError(ExtentTest logger)
        {
            webDriver.WaitForElementToBeVisible(WifiSignIn.accountNotFoundError, 10);
            var text = webDriver.GetElementText(WifiSignIn.accountNotFoundError);
            logger?.Log(Status.Info, $"Account not found: {text}");
            return text;
        }
    }
}
