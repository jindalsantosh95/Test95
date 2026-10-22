using AventStack.ExtentReports;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.MyAccount.UI.ElementsLibrary;

namespace VAA.EAPI.Automation.MyAccount.UI.PageObjects
{
    public class WifiOtpPage
    {
        private readonly IWebDriver webDriver;

        public WifiOtpPage(IWebDriver driver)
        {
            webDriver = driver;
        }
        public void EnterOtp(ExtentTest logger, int otp)
        {
            var element = webDriver.FindElement(WifiOtp.otpField, "OTP input field");
            element.Click();

            // Use JS to set value and dispatch events for React numeric input fields
            var js = (IJavaScriptExecutor)webDriver;
            js.ExecuteScript(
                "var el = arguments[0]; " +
                "var nativeInputValueSetter = Object.getOwnPropertyDescriptor(window.HTMLInputElement.prototype, 'value').set; " +
                "nativeInputValueSetter.call(el, arguments[1]); " +
                "el.dispatchEvent(new Event('input', { bubbles: true })); " +
                "el.dispatchEvent(new Event('change', { bubbles: true }));",
                element, otp.ToString());

            logger?.Log(Status.Info, $"Entered OTP: {otp} from storage table");
        }

        public void ClickVerify(ExtentTest logger)
        {
            webDriver.FindElementAndClick(WifiOtp.verifyButton, "Verify button");
            logger?.Log(Status.Info, "Clicked Verify");
        }
    }
}
