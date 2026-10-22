using System.Globalization;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.UI.Factory;
using VAA.EAPI.Automation.Core.UI.PageObjects.Holidays;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.Holidays.GraphQL.Payments
{
    public class HolidayPartialPaymentTests
    {
        private readonly string _env = Config.Environment;
        private const int MaxAttempts = 2;

        private const string HolidayPaymentCardName = "FL BRWA";
        private const string HolidayPaymentCardNumber = "5333302221254276";
        private const string HolidayPaymentCardExpiry = "1229";
        private const string HolidayPaymentCardCvv = "123";
        private const string HolidayPaymentAddressLineOne = "THE VHQ";
        private const string HolidayPaymentAddressLineTwo = "addressline Two";
        private const string HolidayPaymentTownCity = "Crawley";
        private const string HolidayPaymentCountry = "UK";
        private const string HolidayPaymentPostcode = "CV47 2TH";


        [SkippableFact]
        [RunOrder(1)]
        [Trait("Category", Traits.HolidaysPaymentRegression)]
        public async Task HolidayPaymentMutation_PartialAmount_ReturnsPendingAndSetsCookies()
        {
            Exception? lastException = null;

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    #region TC12-Step1-Capture-Partial-Payment-Session
                    (string paymentLink, string mmbSessionCookieValue, string virginHolidaysPaymentCookieValue) =
                        await CapturePartialPaymentSessionAsync(attempt, MaxAttempts);
                    #endregion

                    #region TC12-Step2-Complete-HPP-Flow-With-Captured-Cookies
                    ExecuteHppFlow(paymentLink, mmbSessionCookieValue, virginHolidaysPaymentCookieValue, attempt, MaxAttempts);
                    #endregion

                    return;
                }
                catch (Exception ex) when (ex.GetType().Name.Contains("SkipException", StringComparison.OrdinalIgnoreCase))
                {
                    throw;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            throw new Exception($"TC12 failed after {MaxAttempts} attempts. Last error: {lastException?.Message}", lastException);
        }

        private async Task<(string paymentLink, string mmbSessionCookieValue, string virginHolidaysPaymentCookieValue)> CapturePartialPaymentSessionAsync(int attempt, int maxAttempts)
        {
            string attemptContext = $"Attempt {attempt} of {maxAttempts}";
            string departureDate = HolidaysHelper.GenerateFutureDepartureDate();
            string[] optionStatuses = { "FIRM", "DEFINITE" };
            Dictionary<string, string> bookingDetails = HolidaysHelper.RetrieveHolidayBookingDetailsByOptionStatus(optionStatuses, departureDate, out string selectedOptionStatus);

            TestDataPrerequisiteHelper.ValidateHolidayDetailsOrSkip(bookingDetails, requireItems: false, requireDepartureDate: true);

            string bookingReference = bookingDetails["bookingReference"];
            string dateOfBirth = HolidaysHelper.NormalizeDateOfBirth(bookingDetails["dateOfBirth"]);
            string leadName = bookingDetails["leadName"];
            string surname = HolidaysHelper.ExtractSurname(leadName);
            string debugContext = $"{HolidaysHelper.FormatHolidayBookingContext(bookingReference, leadName, dateOfBirth)}, optionStatus='{selectedOptionStatus}', bookingStatus='CONFIRMED', {attemptContext}";

            HttpResponseMessage retrieveHolidayBookingResponse = GraphQlEndpoint
                .RetrieveHolidayBooking(_env, "HOLIDAYS", bookingReference, dateOfBirth, surname)
                .EnsureGQLResponseSuccess();

            string retrieveHolidayBookingBody = await retrieveHolidayBookingResponse.Content.ReadAsStringAsync();
            Assert.False(retrieveHolidayBookingBody.IsNullOrWhiteSpace(), $"RetrieveHolidayBooking response body was empty. {debugContext}");

            JObject retrieveHolidayBookingResult = JObject.Parse(retrieveHolidayBookingBody);
            Assert.False(DynamicHelper.HasProperty(retrieveHolidayBookingResult, "errors"), $"Expected no GraphQL errors in RetrieveHolidayBooking response. {debugContext}");

            JObject retrieveHolidayBooking = (JObject)retrieveHolidayBookingResult.RequireToken("data.retrieveHolidayBooking");
            string tripResourceId = retrieveHolidayBooking.RequireToken("tripResourceId")?.ToString() ?? string.Empty;
            Assert.False(tripResourceId.IsNullOrWhiteSpace(), $"tripResourceId should not be null or empty. {debugContext}");

            JObject payment = (JObject)retrieveHolidayBooking.RequireToken("payment");
            Assert.True(decimal.TryParse(payment["balanceDue"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal balanceDue),
                $"payment.balanceDue should be numeric. Actual='{payment["balanceDue"]}'. {debugContext}");
            Assert.True(balanceDue > 0, $"payment.balanceDue should be > 0 for partial payment flow. balanceDue='{balanceDue}'. {debugContext}");

            HttpResponseMessage holidayPaymentMutationResponse = GraphQlEndpoint
                .HolidayPaymentMutation(_env, tripResourceId, 1m)
                .EnsureGQLResponseSuccess();

            string holidayPaymentMutationBody = await holidayPaymentMutationResponse.Content.ReadAsStringAsync();
            Assert.False(holidayPaymentMutationBody.IsNullOrWhiteSpace(), $"HolidayPaymentMutation response body was empty. {debugContext}, tripResourceId='{tripResourceId}'");

            JObject holidayPaymentMutationResult = JObject.Parse(holidayPaymentMutationBody);
            Assert.False(DynamicHelper.HasProperty(holidayPaymentMutationResult, "errors"),
                $"Expected no GraphQL errors in HolidayPaymentMutation response. {debugContext}, tripResourceId='{tripResourceId}'");

            JObject holidayPaymentMutation = (JObject)holidayPaymentMutationResult.RequireToken("data.holidayPaymentMutation");

            Assert.True(decimal.TryParse(holidayPaymentMutation["amountToPay"]?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal amountToPay),
                $"holidayPaymentMutation.amountToPay should be numeric. Actual='{holidayPaymentMutation["amountToPay"]}'. {debugContext}");
            Assert.Equal(1m, amountToPay);

            string paymentStatus = holidayPaymentMutation["status"]?.ToString() ?? string.Empty;
            Assert.True(paymentStatus.Equals("PENDING", StringComparison.OrdinalIgnoreCase),
                $"holidayPaymentMutation.status should be 'PENDING'. Actual='{paymentStatus}'. {debugContext}");

            string paymentLink = holidayPaymentMutation["paymentLink"]?.ToString() ?? string.Empty;
            string removeLink = holidayPaymentMutation["removeLink"]?.ToString() ?? string.Empty;

            Assert.False(paymentLink.IsNullOrWhiteSpace(), $"holidayPaymentMutation.paymentLink should not be null or empty. {debugContext}");
            Assert.False(removeLink.IsNullOrWhiteSpace(), $"holidayPaymentMutation.removeLink should not be null or empty. {debugContext}");
            Assert.True(Uri.TryCreate(paymentLink, UriKind.Absolute, out _), $"holidayPaymentMutation.paymentLink should be a valid absolute URI. Actual='{paymentLink}'. {debugContext}");
            Assert.True(Uri.TryCreate(removeLink, UriKind.Absolute, out _), $"holidayPaymentMutation.removeLink should be a valid absolute URI. Actual='{removeLink}'. {debugContext}");

            Dictionary<string, string> cookies = HolidaysHelper.ExtractCookiesFromResponse(holidayPaymentMutationResponse);
            Assert.True(cookies.TryGetValue("mmb-session", out string? mmbSessionCookieValue) && !mmbSessionCookieValue.IsNullOrWhiteSpace(),
                $"Expected mmb-session cookie in Set-Cookie header. {debugContext}");
            Assert.True(cookies.TryGetValue("com.virginholidays.payment.v2", out string? virginHolidaysPaymentCookieValue) && !virginHolidaysPaymentCookieValue.IsNullOrWhiteSpace(),
                $"Expected com.virginholidays.payment.v2 cookie in Set-Cookie header. {debugContext}");

            return (paymentLink, mmbSessionCookieValue, virginHolidaysPaymentCookieValue);
        }

        private static void ExecuteHppFlow(string paymentLink, string mmbSessionCookieValue, string virginHolidaysPaymentCookieValue, int attempt, int maxAttempts)
        {
            Assert.False(paymentLink.IsNullOrWhiteSpace(), $"paymentLink was not captured. Attempt {attempt} of {maxAttempts}.");
            Assert.False(mmbSessionCookieValue.IsNullOrWhiteSpace(), $"mmb-session cookie value was not captured. Attempt {attempt} of {maxAttempts}.");
            Assert.False(virginHolidaysPaymentCookieValue.IsNullOrWhiteSpace(), $"com.virginholidays.payment.v2 cookie value was not captured. Attempt {attempt} of {maxAttempts}.");
            Assert.True(Uri.TryCreate(paymentLink, UriKind.Absolute, out _), $"Captured paymentLink is invalid. paymentLink='{paymentLink}'. Attempt {attempt} of {maxAttempts}.");

            IWebDriver? webDriver = null;

            try
            {
                webDriver = BrowserFactory.GetBrowser(BrowserFactory.BrowserType.Chrome, paymentLink, true, true);

                webDriver.Manage().Cookies.AddCookie(new Cookie("mmb-session", mmbSessionCookieValue, "/", null));
                webDriver.Manage().Cookies.AddCookie(new Cookie("com.virginholidays.payment.v2", virginHolidaysPaymentCookieValue, "/", null));

                webDriver.Navigate().Refresh();

                CpdForm cpdForm = new(webDriver);
                Assert.True(cpdForm.CheckCardDisplayed(out ValidationResultHelper result),
                    $"HPP card section was not displayed after cookie injection. paymentLink='{paymentLink}'. Attempt {attempt} of {maxAttempts}. Error: {string.Join(", ", result.Message)}");

                cpdForm.ClickOnCard();
                cpdForm.EnterNameOnCard(HolidayPaymentCardName);
                cpdForm.EnterCardNumber(HolidayPaymentCardNumber);
                cpdForm.EnterExpiryDate(HolidayPaymentCardExpiry);
                cpdForm.EnterCvv(HolidayPaymentCardCvv);
                cpdForm.EnterAddressLine1(HolidayPaymentAddressLineOne);
                cpdForm.EnterAddressLine2(HolidayPaymentAddressLineTwo);
                cpdForm.EnterTownCity(HolidayPaymentTownCity);
                cpdForm.SelectCountry(HolidayPaymentCountry);
                cpdForm.EnterPostcode(HolidayPaymentPostcode);
                cpdForm.ClickPayAndConfirmBooking();
                cpdForm.CheckPaymentCompleted();
            }
            finally
            {
                webDriver?.DriverCleanUp();
            }
        }
    }
}
