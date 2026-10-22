using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MyAccount.UI.PageObjects
{
    public class WifiPasswordResetPage
    {
        private readonly IWebDriver webDriver;

        public WifiPasswordResetPage(IWebDriver driver)
        {
            webDriver = driver;
        }

        public void EnterEmail(ExtentTest logger, string email)
        {
            webDriver.FindElementAndEnterText(WifiPasswordReset.emailField, email, "Password reset email field");
            logger?.Log(Status.Info, $"Entered email for password reset: {email}");
        }

        public void SubmitEmail(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiPasswordReset.nextButton, "Submit email for reset");
            logger?.Log(Status.Info, "Submitted email for password reset");
        }

        public void EnterNewPassword(ExtentTest logger, string password)
        {
            webDriver.FindElementAndEnterText(WifiPasswordReset.newPasswordField, password, "New password field");
            logger?.Log(Status.Info, "Entered new password");
        }

        public void ConfirmNewPassword(ExtentTest logger, string password)
        {
            webDriver.FindElementAndEnterText(WifiPasswordReset.confirmPasswordField, password, "Confirm password field");
            logger?.Log(Status.Info, "Confirmed new password");
        }

        public void ClickChangePassword(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiPasswordReset.changePasswordButton, "Change password button");
            logger?.Log(Status.Info, "Clicked Change Password");
        }

        public string GetSuccessMessage(ExtentTest logger)
        {
            webDriver.WaitForElementToBeVisible(WifiPasswordReset.connectedMessage);
            logger?.Log(Status.Info, "Password reset success and connected message displayed");
            return webDriver.GetElementText(WifiPasswordReset.connectedMessage);
        }

        public void TabOutOfNewPassword(ExtentTest logger)
        {
            webDriver.FindElement(WifiPasswordReset.newPasswordField, "New password field").SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Tabbed out of new password field");
        }

        public void TabOutOfConfirmPassword(ExtentTest logger)
        {
            webDriver.FindElement(WifiPasswordReset.confirmPasswordField, "Confirm password field").SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Tabbed out of confirm password field");
        }

        /// <summary>
        /// Checks whether a password validation rule is met (green tick) or unmet (transparent tick).
        /// The tick span has class 'text-transparent' when unmet.
        /// </summary>
        /// <param name="ruleLocator">XPath to the rule's &lt;li&gt; element (e.g. WifiPasswordReset.passwordLengthRule)</param>
        /// <returns>True if the rule is met (tick is visible), false if unmet (tick is transparent)</returns>
        public bool IsPasswordRuleMet(ExtentTest logger, string ruleLocator, string ruleName)
        {
            webDriver.WaitForElementToBeVisible(ruleLocator, 5);
            var tickLocator = ruleLocator + WifiPasswordReset.tickIcon.TrimStart('.');
            var tickElement = webDriver.FindElement(tickLocator, $"Tick icon for rule '{ruleName}'", 5);
            var tickClass = tickElement.GetAttribute("class");

            bool isMet = !tickClass.Contains("text-transparent");
            logger?.Log(Status.Info, $"Rule '{ruleName}': {(isMet ? "MET" : "NOT MET")}");
            return isMet;
        }
    }
}
