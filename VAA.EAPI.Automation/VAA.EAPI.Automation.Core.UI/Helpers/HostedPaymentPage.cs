using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.Core.UI.PageObjects.DotCom;
using Xunit;
using static VAA.EAPI.Automation.Core.UI.Factory.BrowserFactory;

namespace VAA.EAPI.Automation.Core.UI.Helpers
{
    public class NavigateToHostedPaymentPage : IDisposable
    {
        private IWebDriver webDriver;
        private HppForm hppForm;

        public NavigateToHostedPaymentPage
            (string url, bool customHeaders = true, BrowserType browserType = BrowserType.Chrome)
        {
            webDriver = GetBrowser(browserType, url, true, customHeaders);

            hppForm = new HppForm(webDriver);
        }

        public void ValidateHPPLoaded(double totalSeatPrice, bool rewardFlag = false)
        {
            string url = webDriver.Url.ToString();

            Assert.True(hppForm.CheckCardDisplayed(out var result), $"URL = {url} \r\n Error found. Error: " + string.Join(", ", result.Message));

            if (rewardFlag == true || totalSeatPrice > 0)
            {
                Assert.True(hppForm.CheckPayPalNotDisplayed(out result), $"URL = {url} \r\n Error found. Error: " + string.Join(", ", result.Message));
            }
            else
            {
                Assert.True(hppForm.CheckPayPalDisplayed(out result), $"URL = {url} \r\n Error found. Error: " + string.Join(", ", result.Message));
            }
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'SubmitHppCardDetails(TestConfig config)'.")]
        public void SubmitHppCardDetails(string nameOnCard, string cardNumber, string expiryDate, string cvv, string addressLine1, string addressLine2, string townCity, string country, string postcode, bool threeDs = false, bool rewardFlag = false, string state = "")
        {
            hppForm.ClickOnCard();

            hppForm.EnterNameOnCard(nameOnCard);

            hppForm.EnterCardNumber(cardNumber);

            hppForm.EnterExpiryDate(expiryDate);

            hppForm.EnterCvv(cvv);

            hppForm.EnterAddressLine1(addressLine1);

            hppForm.EnterAddressLine2(addressLine2);

            hppForm.EnterTownCity(townCity);

            hppForm.EnterCountry(country);

            if (!state.IsNullOrWhiteSpace())
                hppForm.EnterState(state);

            hppForm.EnterPostcode(postcode);

            hppForm.ClickPayAndConfirmBooking();

            if (threeDs)
            {
                hppForm.EnterOTP("1234");
                hppForm.SubmitOTP();
            }

            hppForm.CheckPaymentCompleted();
        }

        public void SubmitHppCardDetails(TestConfig config)
        {
            hppForm.ClickOnCard();

            hppForm.EnterNameOnCard(config.Book.Payment.CardDetails.NameOnCard);

            hppForm.EnterCardNumber(config.Book.Payment.CardDetails.CardNumber);

            hppForm.EnterExpiryDate(config.Book.Payment.CardDetails.ExpiryDate);

            hppForm.EnterCvv(config.Book.Payment.CardDetails.Cvv);

            hppForm.EnterAddressLine1(config.Book.Payment.CardDetails.AddressLineOne);

            hppForm.EnterAddressLine2(config.Book.Payment.CardDetails.AddressLineTwo);

            hppForm.EnterTownCity(config.Book.Payment.CardDetails.TownCity);

            hppForm.EnterCountry(config.Book.Payment.CardDetails.Country);

            if (!config.Book.Payment.CardDetails.State.IsNullOrWhiteSpace())
                hppForm.EnterState(config.Book.Payment.CardDetails.State);

            hppForm.EnterPostcode(config.Book.Payment.CardDetails.Postcode);

            hppForm.ClickPayAndConfirmBooking();

            if (config.Book.Payment.CardDetails.IsThreeDs)
            {
                hppForm.EnterOTP("1234");
                hppForm.SubmitOTP();
            }

            hppForm.CheckPaymentCompleted();
        }

        public void SubmitAndVerifyHppCardDetails(TestConfig config)
        {
            hppForm.ClickOnCard();

            hppForm.ClickPayAndConfirmBooking();

            hppForm.CheckPaymentCompleted();
        }

        public void SubmitPayPal(TestConfig config)
        {
            hppForm.ClickOnPayPal();

            hppForm.ClickOnPayWithPayPal();

            hppForm.EnterEmailOrMobileNumber(config.Book.Payment.APMDetails.Email);

            hppForm.ClickNextButton();

            hppForm.EnterPassword(config.Book.Payment.APMDetails.Password);

            hppForm.ClickLogin();

            hppForm.CompletePurchase();
        }

        public void SubmitHppCardDetailsDeclined(string nameOnCard, string cardNumber, string expiryDate, string cvv, string addressLine1, string addressLine2, string townCity, string country, string postcode)
        {
            hppForm.ClickOnCard();

            //First try
            hppForm.EnterNameOnCard(nameOnCard);

            hppForm.EnterCardNumber(cardNumber);

            hppForm.EnterExpiryDate(expiryDate);

            hppForm.EnterCvv(cvv);

            hppForm.EnterAddressLine1(addressLine1);

            hppForm.EnterAddressLine2(addressLine2);

            hppForm.EnterTownCity(townCity);

            hppForm.EnterCountry(country);

            hppForm.EnterPostcode(postcode);

            hppForm.ClickPayAndConfirmBooking();

            hppForm.CheckErrorDisplayed();

            //Second try
            hppForm.EnterNameOnCard(nameOnCard);

            hppForm.EnterCardNumber(cardNumber);

            hppForm.EnterExpiryDate(expiryDate);

            hppForm.EnterCvv(cvv);

            hppForm.EnterAddressLine1(addressLine1);

            hppForm.EnterAddressLine2(addressLine2);

            hppForm.EnterTownCity(townCity);

            hppForm.EnterCountry(country);

            hppForm.EnterPostcode(postcode);

            hppForm.ClickPayAndConfirmBooking();

            hppForm.CheckErrorDisplayed();

            //Third try
            hppForm.EnterNameOnCard(nameOnCard);

            hppForm.EnterCardNumber(cardNumber);

            hppForm.EnterExpiryDate(expiryDate);

            hppForm.EnterCvv(cvv);

            hppForm.EnterAddressLine1(addressLine1);

            hppForm.EnterAddressLine2(addressLine2);

            hppForm.EnterTownCity(townCity);

            hppForm.EnterCountry(country);

            hppForm.EnterPostcode(postcode);

            hppForm.ClickPayAndConfirmBooking();

            hppForm.CheckFailureRedirect();
        }

        public void SubmitDetailsForSynchronyCardPayment()
        {
            hppForm.ClickPayAndConfirmBooking();

            hppForm.CheckPaymentCompleted();
        }

        public string GetChildPri()
        {
            var childPri = hppForm.GetChildPri();

            return childPri;
        }

        public void Dispose()
        {
            // Close browser
            webDriver.DriverCleanUp();
            webDriver.Dispose();
            webDriver = null;
        }
    }
}
