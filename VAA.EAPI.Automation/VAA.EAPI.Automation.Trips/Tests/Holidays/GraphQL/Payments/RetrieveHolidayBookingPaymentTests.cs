using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;

namespace VAA.EAPI.Automation.Trips.Tests.Holidays.GraphQL.Payments
{
    public class RetrieveHolidayBookingPaymentTests
    {
        private readonly string _env = Config.Environment;
        public static IEnumerable<object[]> PaymentOptionStatuses()
        {
            yield return new object[] { "FIRM" };
            yield return new object[] { "DEFINITE" };
            yield return new object[] { "FULLY PAID" };
        }

        [SkippableTheory]
        [Trait("Category", Traits.HolidaysPaymentRegression)]
        [MemberData(nameof(PaymentOptionStatuses), MemberType = typeof(RetrieveHolidayBookingPaymentTests))]
        public async Task RetrieveHolidayBooking_PaymentSection_MapsAttributes(string optionStatus)
        {
            (JObject retrieveHolidayBooking, string debugContext) = await HolidaysHelper.RetrieveHolidayBookingForPaymentAsync(_env, optionStatus);

            HolidaysHelper.ValidatePaymentSection(retrieveHolidayBooking, debugContext);
        }

        [SkippableTheory]
        [Trait("Category", Traits.HolidaysPaymentRegression)]
        [MemberData(nameof(PaymentOptionStatuses), MemberType = typeof(RetrieveHolidayBookingPaymentTests))]
        public async Task RetrieveHolidayBooking_PaymentSummary_ReturnsRequiredUiFields(string optionStatus)
        {
            JObject payment = await RetrievePaymentObject(optionStatus);

            Assert.NotNull(payment.Property("balanceDue"));
            Assert.NotNull(payment.Property("totalPriceToBePaid"));
            Assert.NotNull(payment.Property("totalPaidAmount"));
            Assert.NotNull(payment.Property("paidPercentage"));
            Assert.NotNull(payment.Property("dueDate"));

            Assert.True(decimal.TryParse(payment["balanceDue"]?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _), $"payment.balanceDue should be numeric. Actual='{payment["balanceDue"]}'. optionStatus='{optionStatus}'.");

            Assert.True(decimal.TryParse(payment["totalPriceToBePaid"]?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _), $"payment.totalPriceToBePaid should be numeric. Actual='{payment["totalPriceToBePaid"]}'. optionStatus='{optionStatus}'.");

            Assert.True(decimal.TryParse(payment["totalPaidAmount"]?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out _), $"payment.totalPaidAmount should be numeric. Actual='{payment["totalPaidAmount"]}'. optionStatus='{optionStatus}'.");

            Assert.True(int.TryParse(payment["paidPercentage"]?.ToString(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int paidPercentage), $"payment.paidPercentage should be an integer. Actual='{payment["paidPercentage"]}'. optionStatus='{optionStatus}'.");

            Assert.True(paidPercentage is >= 0 and <= 100, $"payment.paidPercentage should be between 0 and 100. Actual='{paidPercentage}'. optionStatus='{optionStatus}'.");
        }


        [SkippableTheory]
        [Trait("Category", Traits.HolidaysPaymentRegression)]
        [MemberData(nameof(PaymentOptionStatuses), MemberType = typeof(RetrieveHolidayBookingPaymentTests))]
        public async Task RetrieveHolidayBooking_PaymentSummary_DueDateMatchesBalanceDueState(string optionStatus)
        {
            JObject payment = await RetrievePaymentObject(optionStatus);

            Assert.True(decimal.TryParse(payment["balanceDue"]?.ToString(), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal balanceDue), $"payment.balanceDue should be numeric. Actual='{payment["balanceDue"]}'. optionStatus='{optionStatus}'.");



            string dueDate = payment["dueDate"]?.ToString() ?? string.Empty;

            if (balanceDue > 0)
                Assert.False(string.IsNullOrWhiteSpace(dueDate), $"payment.dueDate should be populated when balanceDue > 0. balanceDue='{balanceDue}'. optionStatus='{optionStatus}'.");


            if (!string.IsNullOrWhiteSpace(dueDate))
                Assert.True(DateTime.TryParseExact(dueDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out _),
                    $"payment.dueDate should be in yyyy-MM-dd format when populated. Actual='{dueDate}'. optionStatus='{optionStatus}'.");
        }

        private async Task<JObject> RetrievePaymentObject(string optionStatus)
        {
            (JObject retrieveHolidayBooking, string debugContext) = await HolidaysHelper.RetrieveHolidayBookingForPaymentAsync(_env, optionStatus);
            JToken paymentToken = retrieveHolidayBooking.RequireToken("payment");
            JObject payment = paymentToken as JObject;
            Assert.True(payment != null, $"retrieveHolidayBooking.payment should be an object. {debugContext}");

            return payment;

        }
    }
}
