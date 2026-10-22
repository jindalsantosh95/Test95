using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MyAccount.UI.PageObjects
{
    public class WifiSignUpPage
    {
        private readonly IWebDriver webDriver;

        public WifiSignUpPage(IWebDriver driver)
        {
            webDriver = driver;
        }

        public void EnterFirstName(ExtentTest logger, string firstName)
        {
            webDriver.FindElementAndEnterText(WifiSignUp.firstNameField, firstName, "First name field");
            logger?.Log(Status.Info, $"Entered first name: {firstName}");
        }

        public void EnterLastName(ExtentTest logger, string lastName)
        {
            webDriver.FindElementAndEnterText(WifiSignUp.lastNameField, lastName, "Last name field");
            logger?.Log(Status.Info, $"Entered last name: {lastName}");
        }

        public void EnterDateOfBirth(ExtentTest logger, string dob)
        {
            var element = webDriver.FindElement(WifiSignUp.dateOfBirthField, "Date of birth field");
            element.Click();

            // Use JS to set value and dispatch input/change events for React/Angular fields
            var js = (IJavaScriptExecutor)webDriver;
            js.ExecuteScript(
                "var el = arguments[0]; " +
                "var nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set; " +
                "nativeInputValueSetter.call(el, arguments[1]); " +
                "el.dispatchEvent(new Event('input', { bubbles: true })); " +
                "el.dispatchEvent(new Event('change', { bubbles: true }));",
                element, dob);

            logger?.Log(Status.Info, $"Entered date of birth: {dob}");
        }

        public void EnterEmail(ExtentTest logger, string email)
        {
            webDriver.FindElementAndEnterText(WifiSignUp.emailField, email, "Email field");
            logger?.Log(Status.Info, $"Entered email: {email}");
        }

        public void Next(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiSignUp.nextButton, "Next button");
            logger?.Log(Status.Info, "Clicked Next button");
        }

        public void Submit(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiSignUp.confirmButton, "Sign up submit button");
            logger?.Log(Status.Info, "Clicked Sign Up Submit");
        }

        public void ConnectNow(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiSignUp.connectNow, "Connect now button");
            logger?.Log(Status.Info, "Clicked Connect now Submit");
        }

        public string GetConnectedMessage(ExtentTest logger)
        {
            webDriver.WaitForElementToBeVisible(WifiSignUp.connectedMessage);
            logger?.Log(Status.Info, "Connected message displayed after sign up");
            return webDriver.GetElementText(WifiSignUp.connectedMessage);
        }

        public void TabOutOfFirstName(ExtentTest logger)
        {
            webDriver.FindElement(WifiSignUp.firstNameField, "First name field").SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Tabbed out of first name field");
        }

        public void TabOutOfLastName(ExtentTest logger)
        {
            webDriver.FindElement(WifiSignUp.lastNameField, "Last name field").SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Tabbed out of last name field");
        }

        public void TabOutOfDateOfBirth(ExtentTest logger)
        {
            webDriver.FindElement(WifiSignUp.dateOfBirthField, "Date of birth field").SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Tabbed out of date of birth field");
        }

        public void ClearFirstName(ExtentTest logger)
        {
            var element = webDriver.FindElement(WifiSignUp.firstNameField, "First name field");
            element.Click();
            element.SendKeys("a");
            element.SendKeys(Keys.Control + "a");
            element.SendKeys(Keys.Backspace);
            element.SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Cleared first name field");
        }

        public void ClearLastName(ExtentTest logger)
        {
            var element = webDriver.FindElement(WifiSignUp.lastNameField, "Last name field");
            element.Click();
            element.SendKeys("a");
            element.SendKeys(Keys.Control + "a");
            element.SendKeys(Keys.Backspace);
            element.SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Cleared last name field");
        }

        public void ClearDateOfBirth(ExtentTest logger)
        {
            var element = webDriver.FindElement(WifiSignUp.dateOfBirthField, "Date of birth field");
            element.Click();
            element.SendKeys("a");
            element.SendKeys(Keys.Control + "a");
            element.SendKeys(Keys.Backspace);
            element.SendKeys(Keys.Tab);
            logger?.Log(Status.Info, "Cleared date of birth field");
        }

        public string GetValidationError(ExtentTest logger, string errorLocator, string description)
        {
            webDriver.WaitForElementToBeVisible(errorLocator, 5);
            var text = webDriver.GetElementText(errorLocator);
            logger?.Log(Status.Info, $"{description}: {text}");
            return text;
        }
    }
}
