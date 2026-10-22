using System.Diagnostics;
using System.Text;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.ElementLibrary;
using VAA.EAPI.Automation.Core.UI.Extensions;
using VAA.EAPI.Automation.Core.UI.Factory;

namespace VAA.EAPI.Automation.Core.UI.PageObjects.Holidays
{
    public class CpdForm
    {
        private const int DefaultPaymentRedirectTimeoutSeconds = 90;
        private readonly IWebDriver _webDriver;

        public CpdForm(IWebDriver driver)
        {
            _webDriver = driver;
        }

        public bool CheckCardDisplayed(out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            if (_webDriver.WaitForElementToBeVisible(CpdPaymentPage.Card, 30) == null)
            {
                string screenshotName = $"ErrorScreenCapture_{Config.Environment}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                _webDriver.GetScreenshot(screenshotName);

                string currentUrl = _webDriver.Url;
                string pageTitle = _webDriver.Title;
                string consoleErrors = BrowserFactory.ReturnBrowserLogs(_webDriver);
                string pageSource = _webDriver.PageSource?.Length > 500
                    ? _webDriver.PageSource[..500] + "... (truncated)"
                    : _webDriver.PageSource;

                result.Message.Add(
                    $"CPD Load Failure. \r\n Card option was not displayed." +
                    $"\r\n Screenshot: {screenshotName}" +
                    $"\r\n Current URL: {currentUrl}" +
                    $"\r\n Page Title: {pageTitle}" +
                    $"\r\n Page Source (first 500 chars): {pageSource}" +
                    $"\r\n Browser Errors logs: {(string.IsNullOrEmpty(consoleErrors) ? "(none)" : consoleErrors)}");
            }

            return result.IsValid;
        }

        public void ClickOnCard()
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.Card, 20);

            _webDriver.FindElementAndClick(CpdPaymentPage.Card, "Card expander", 20);
        }

        public void EnterNameOnCard(string nameOnCard)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.NameOnCard);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.NameOnCard, nameOnCard, "Name on card");
        }

        public void EnterCardNumber(string cardNumber)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.CardNumber);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.CardNumber, cardNumber, "Card number");
        }

        public void EnterExpiryDate(string expiryDate)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.ExpiryDate);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.ExpiryDate, expiryDate, "Expiry date");
        }

        public void EnterCvv(string cvv)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.CVV);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.CVV, cvv, "CVV");
        }

        public void EnterAddressLine1(string addressLine1)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.AddressLine1);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.AddressLine1, addressLine1, "Address Line 1");
        }

        public void EnterAddressLine2(string addressLine2)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.AddressLine2);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.AddressLine2, addressLine2, "Address Line 2");
        }

        public void EnterTownCity(string townCity)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.TownCity);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.TownCity, townCity, "Town / City");
        }

        public void SelectCountry(string country)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.CountryDropdown);

            _webDriver.FindElementAndClick(CpdPaymentPage.CountryDropdown, "Country dropdown");

            string countryOption = CpdPaymentPage.CountryOption.Replace("country", country);

            _webDriver.WaitForElementToBeVisible(countryOption);

            _webDriver.FindElementAndClick(countryOption, $"Country option {country}");
        }

        public void EnterState(string state)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.State);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.State, state, "State").SendKeys(Keys.Tab);
        }

        public void EnterPostcode(string postcode)
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.Postcode);

            _webDriver.FindElementAndEnterText(CpdPaymentPage.Postcode, postcode, "Postcode");
        }

        public void ClickPayAndConfirmBooking()
        {
            _webDriver.WaitForElementToBeVisible(CpdPaymentPage.PayAndConfirmBooking);

            _webDriver.FindElementAndClick(CpdPaymentPage.PayAndConfirmBooking, "PayAndConfirmBooking");
        }

        public void CheckPaymentCompleted()
        {
            _webDriver.WaitForRedirectUrl("payment/success", ResolvePaymentRedirectTimeoutSeconds());
            ValidatePaymentAcceptResponseStatus();
        }

        private void ValidatePaymentAcceptResponseStatus()
        {
            const string acceptEndpointFragment = "/cpd/payment/accept";
            const int networkValidationTimeoutSeconds = 10;

            Stopwatch stopwatch = Stopwatch.StartNew();
            List<string> observedAcceptEvents = new();

            while (stopwatch.Elapsed < TimeSpan.FromSeconds(networkValidationTimeoutSeconds))
            {
                IReadOnlyCollection<OpenQA.Selenium.LogEntry>? performanceLogs = TryGetPerformanceLogs();
                if (performanceLogs is null)
                {
                    throw new InvalidOperationException("This driver does not support performance/network log retrieval required for payment accept status validation.");
                }

                foreach (OpenQA.Selenium.LogEntry logEntry in performanceLogs)
                {
                    if (!TryExtractNetworkEvent(logEntry.Message, out string? url, out int? statusCode, out string? method))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(url)
                        || !url.Contains(acceptEndpointFragment, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    observedAcceptEvents.Add($"method={method ?? "(unknown)"}, status={statusCode?.ToString() ?? "(none)"}, url={url}");

                    if (statusCode == 303)
                    {
                        return;
                    }
                }

                System.Threading.Thread.Sleep(500);
            }

            StringBuilder diagnostics = new();
            diagnostics.Append($"Payment accept call '{acceptEndpointFragment}' did not return HTTP 303 within {networkValidationTimeoutSeconds} seconds.");

            if (observedAcceptEvents.Any())
            {
                diagnostics.Append(" Observed network events: ");
                diagnostics.Append(string.Join(" | ", observedAcceptEvents.TakeLast(10)));
            }
            else
            {
                diagnostics.Append(" No matching accept network events were captured from performance logs.");
            }

            throw new Exception(diagnostics.ToString());
        }

        private static bool TryExtractNetworkEvent(string logMessage, out string? url, out int? statusCode, out string? method)
        {
            url = null;
            statusCode = null;
            method = null;

            try
            {
                JObject root = JObject.Parse(logMessage);
                method = root.SelectToken("$.message.method")?.ToString();

                JToken? responseUrl = root.SelectToken("$.message.params.response.url");
                JToken? responseStatus = root.SelectToken("$.message.params.response.status");

                JToken? redirectUrl = root.SelectToken("$.message.params.redirectResponse.url");
                JToken? redirectStatus = root.SelectToken("$.message.params.redirectResponse.status");

                JToken? requestUrl = root.SelectToken("$.message.params.request.url");

                url = responseUrl?.ToString() ?? redirectUrl?.ToString() ?? requestUrl?.ToString();

                if (responseStatus != null && int.TryParse(responseStatus.ToString(), out int parsedResponseStatus))
                {
                    statusCode = parsedResponseStatus;
                }
                else if (redirectStatus != null && int.TryParse(redirectStatus.ToString(), out int parsedRedirectStatus))
                {
                    statusCode = parsedRedirectStatus;
                }

                return !string.IsNullOrWhiteSpace(url);
            }
            catch
            {
                return false;
            }
        }

        private IReadOnlyCollection<OpenQA.Selenium.LogEntry>? TryGetPerformanceLogs()
        {
            try
            {
                return _webDriver.Manage().Logs.GetLog("performance");
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static int ResolvePaymentRedirectTimeoutSeconds()
        {
            int timeoutInSeconds = ConfigReader.Get("CpdPaymentRedirectTimeoutSeconds", DefaultPaymentRedirectTimeoutSeconds);

            return timeoutInSeconds > 0 ? timeoutInSeconds : DefaultPaymentRedirectTimeoutSeconds;
        }
    }
}
