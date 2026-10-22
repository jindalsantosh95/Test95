using Microsoft.Graph.Models;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.UI.ElementLibrary;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Factory;

namespace VAA.EAPI.Automation.Core.UI.PageObjects.DotCom
{
    public class HppForm
    {
        private const int DefaultPaymentRedirectTimeoutSeconds = 90;
        private readonly IWebDriver _webDriver;

        public HppForm(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public bool CheckCardDisplayed(out ValidationResultHelper result, bool embedded = false)
        {
            result = new ValidationResultHelper();

            if (embedded)
            {
                IWebElement iframeElement1 = _webDriver.FindElement(HPPForm.EmbeddedPaymentIframe, "Payment Iframe");

                _webDriver.SwitchTo().Frame(iframeElement1);
            }

            if (_webDriver.WaitForElementToBeVisible(HPPForm.Card, 30) == null)
            {
                var screenshotName = $"ErrorScreenCapture_{Config.Environment}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                _webDriver.GetScreenshot(screenshotName);

                var currentUrl = _webDriver.Url;
                var pageTitle = _webDriver.Title;
                var consoleErrors = BrowserFactory.ReturnBrowserLogs(_webDriver);
                var pageSource = _webDriver.PageSource?.Length > 500
                    ? _webDriver.PageSource[..500] + "... (truncated)"
                    : _webDriver.PageSource;

                result.Message.Add(
                    $"HPP Load Failure. \r\n Card option was not displayed." +
                    $"\r\n Screenshot: {screenshotName}" +
                    $"\r\n Current URL: {currentUrl}" +
                    $"\r\n Page Title: {pageTitle}" +
                    $"\r\n Page Source (first 500 chars): {pageSource}" +
                    $"\r\n Browser Errors logs: {(string.IsNullOrEmpty(consoleErrors) ? "(none)" : consoleErrors)}");
            }
            return result.IsValid;
        }

        public bool CheckPayPalDisplayed(out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (_webDriver.WaitForElementToBeVisible(HPPForm.PayPal, 20) == null)
            {
                var screenshotName = $"ErrorScreenCapture_{Config.Environment}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                _webDriver.GetScreenshot(screenshotName);

                var consoleErrors = BrowserFactory.ReturnBrowserLogs(_webDriver);
                result.Message.Add(
                    $"HPP Load Failure. \r\n PayPal option was not displayed." +
                    $"\r\n Screenshot: {screenshotName}" +
                    $"\r\n Current URL: {_webDriver.Url}" +
                    $"\r\n Browser Errors logs: {(string.IsNullOrEmpty(consoleErrors) ? "(none)" : consoleErrors)}");
            }
            return result.IsValid;
        }
        public bool CheckPayPalNotDisplayed(out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (_webDriver.WaitForElementToBeVisible(HPPForm.PayPal, timeout: 5) != null)
            {
                var screenshotName = $"ErrorScreenCapture_{Config.Environment}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                _webDriver.GetScreenshot(screenshotName);

                var consoleErrors = BrowserFactory.ReturnBrowserLogs(_webDriver);
                result.Message.Add(
                    $"HPP Load Failure. \r\n PayPal option was displayed when it should not be." +
                    $"\r\n Screenshot: {screenshotName}" +
                    $"\r\n Current URL: {_webDriver.Url}" +
                    $"\r\n Browser Errors logs: {(string.IsNullOrEmpty(consoleErrors) ? "(none)" : consoleErrors)}");
            }
            return result.IsValid;
        }

        public void ClickOnCard()
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.Card, 20);

            _webDriver.FindElementAndClick(HPPForm.Card, "Card expander", 20);
        }

        public void EnterNameOnCard(string nameOnCard)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.NameOnCard);

            _webDriver.FindElementAndEnterText(HPPForm.NameOnCard, nameOnCard, "Name on card");
        }

        public void EnterCardNumber(string cardNumber)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.CardNumber);

            _webDriver.FindElementAndEnterText(HPPForm.CardNumber, cardNumber, "Card number");
        }

        public void EnterExpiryDate(string expiryDate)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.ExpiryDate);

            _webDriver.FindElementAndEnterText(HPPForm.ExpiryDate, expiryDate, "Expiry date");
        }

        public void EnterCvv(string cvv)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.CVV);

            _webDriver.FindElementAndEnterText(HPPForm.CVV, cvv, "CVV");
        }

        public void EnterAddressLine1(string addressLine1)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.AddressLine1);

            _webDriver.FindElementAndEnterText(HPPForm.AddressLine1, addressLine1, "Address Line 1");
        }

        public void EnterAddressLine2(string addressLine2)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.AddressLine2);

            _webDriver.FindElementAndEnterText(HPPForm.AddressLine2, addressLine2, "Address Line 2");
        }

        public void EnterTownCity(string townCity)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.TownCity);

            _webDriver.FindElementAndEnterText(HPPForm.TownCity, townCity, "Town / City");
        }

        public void EnterCountry(string country)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.Country);

            _webDriver.FindElementAndEnterText(HPPForm.Country, country, "Country").SendKeys(Keys.Tab);
        }

        public void EnterState(string state)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.State);

            _webDriver.FindElementAndEnterText(HPPForm.State, state, "State").SendKeys(Keys.Tab);
        }

        public void EnterPostcode(string postcode)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.Postcode);

            _webDriver.FindElementAndEnterText(HPPForm.Postcode, postcode, "Postcode");
        }

        public void ClickPayAndConfirmBooking()
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.PayAndConfirmBooking);

            _webDriver.FindElementAndClick(HPPForm.PayAndConfirmBooking, "PayAndConfirmBooking");
        }

        public void EnterOTP(string otpNumber)
        {
            IWebElement iframeElement1 = _webDriver.FindElement(HPPForm.OTPIFrame2, "iframe2");

            _webDriver.SwitchTo().Frame(iframeElement1);

            IWebElement iframeElement2 = _webDriver.FindElement(HPPForm.OTPIFrame3, "iframe3");

            _webDriver.SwitchTo().Frame(iframeElement2);

            _webDriver.WaitForElementToBeVisible(HPPForm.OTPInput);

            _webDriver.FindElementAndEnterText(HPPForm.OTPInput, otpNumber, "OTP Input");
        }

        public void SubmitOTP()
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.OTPSubmitButton);

            _webDriver.FindElementAndClick(HPPForm.OTPSubmitButton, "OTP Submit Button");
        }

        public void CheckPaymentCompleted()
        {
            _webDriver.WaitForRedirectUrl("captured", ResolvePaymentRedirectTimeoutSeconds());
        }

        public void CheckErrorDisplayed()
        {
            _webDriver.FindElement(HPPForm.ErrorMessage, "Error message");
        }

        public void CheckFailureRedirect()
        {
            _webDriver.WaitForRedirectUrl("google", 20);
        }

        private static int ResolvePaymentRedirectTimeoutSeconds()
        {
            int timeoutInSeconds = ConfigReader.Get("HppPaymentRedirectTimeoutSeconds", DefaultPaymentRedirectTimeoutSeconds);

            if (timeoutInSeconds > 0)
            {
                return timeoutInSeconds;
            }

            return DefaultPaymentRedirectTimeoutSeconds;
        }

        public void ClickOnPayPal()
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.PayPal);

            _webDriver.FindElementAndClick(HPPForm.PayPal, "PayPal expander");
        }

        public void ClickOnPayWithPayPal()
        {
            _webDriver.FindElementAndClick(HPPForm.PayWithPayPal, "Pay with PayPal button");
        }

        public void EnterEmailOrMobileNumber(string email)
        {
            _webDriver.SwitchTo().Window(_webDriver.WindowHandles.Last());

            _webDriver.WaitForElementToBeVisible(HPPForm.PayPalEmailOrMobileNumber);

            _webDriver.FindElementAndEnterText(HPPForm.PayPalEmailOrMobileNumber, email, "Email or Mobile Number");
        }

        public void ClickNextButton()
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.NextButton);

            _webDriver.FindElementAndClick(HPPForm.NextButton, "Next button");
        }

        public void EnterPassword(string password)
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.Password);

            _webDriver.FindElementAndEnterText(HPPForm.Password, password, "Password");
        }
        public void ClickLogin()
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.LogIn);

            _webDriver.FindElementAndClick(HPPForm.LogIn, "Login button");
        }


        public void CompletePurchase()
        {
            _webDriver.WaitForElementToBeVisible(HPPForm.CompletePurchase);

            _webDriver.FindElementAndClick(HPPForm.CompletePurchase, "Complete Purcahase");
        }

        public string GetChildPri()
        {
            Thread.Sleep(20000);

            _webDriver.SwitchTo().Window(_webDriver.WindowHandles.Last());

            var url = _webDriver.Url;
            string pri = url.Split('=')[1];

            return pri;
        }
    }
}
