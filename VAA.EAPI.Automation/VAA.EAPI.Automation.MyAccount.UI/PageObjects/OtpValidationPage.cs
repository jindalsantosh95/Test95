using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Helpers;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MyAccount.UI.PageObjects
{
    public class OtpValidationPage
    {
        private IWebDriver webDriver;

        public OtpValidationPage(IWebDriver driver)
        {
            webDriver = driver;
        }

        public void EnterOtp(ExtentTest logger, int Otp)
        {
            webDriver.FindElementAndEnterText(OtpValidation.otpField, Otp.ToString(), "Enter OTP from Storage table");

            logger.Log(Status.Info, "Successfully entered OTP from Storage Table");
        }

        public void ClickOnVerifyButton(ExtentTest logger)
        {
            webDriver.FindElementAndClick(OtpValidation.verifyButton, "Click on Verify button");

            logger.Log(Status.Info, "Successfully clicked on verify button");
        }

        public string GetSentOtpSuccessMessage(ExtentTest logger)
        {
            logger.Log(Status.Info, "Successfully retrieved the success message");

            webDriver.WaitForElementToBeVisible(OtpValidation.successMessage);

            return webDriver.GetElementText(OtpValidation.successMessage);
        }
    }
}
