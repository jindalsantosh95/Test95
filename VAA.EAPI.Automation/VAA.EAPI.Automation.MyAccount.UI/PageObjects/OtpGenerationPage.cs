using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MyAccount.UI.PageObjects
{
    public class OtpGenerationPage
    {
        private IWebDriver webDriver;

        public OtpGenerationPage(IWebDriver driver)
        {
            webDriver = driver;
        }

        public void EnterEmailId(ExtentTest logger, string EmailID)
        {
            webDriver.FindElementAndEnterText(OtpGeneration.emailField, EmailID, "Enter email id from test case");

            logger.Log(Status.Info, "Successfully entered email Id");
        }

        public void ClickOnNextButton(ExtentTest logger)
        {
            webDriver.FindElementAndClick(OtpGeneration.nextButton, "Click on Next button");

            logger.Log(Status.Info, "Successfully clicked on Next button");
        }
    }
}
