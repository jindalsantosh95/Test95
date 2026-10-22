using HotChocolate;
using NodaTime;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    [GraphQLName("HolidayBookingPaymentHistory")]
    public class PaymentHistory
    {
        public decimal? CurrencyAmount { get; set; }
        public LocalDateTime? RealisedDate { get; set; }
        public LocalDateTime? PaidDate { get; set; }

        public string? PaymentCurrency { get; set; }
        public string? PaymentMethodCode { get; set; }
        public string? PaymentMethodDescription { get; set; }
        public bool? Realised { get; set; }
        public LocalDateTime? ReceiptDate { get; set; }
        public PaymentTransactionType TransactionType { get; set; }

        public static explicit operator PaymentHistory((Infrastructure.Models.HolidaysMS.PaymentHistory paymentHistory, IEnumerable<PaymentMethod> refPaymentMethodData) source)
        {
            var (paymentHistory, refPaymentMethodData) = source;

            if (paymentHistory == null) return default;

            return new()
            {
                CurrencyAmount = paymentHistory.CurrencyAmount,
                RealisedDate = paymentHistory.RealisedDate != null ? LocalDateTime.FromDateTime(paymentHistory.RealisedDate.Value) : null,
                TransactionType = paymentHistory.TransactionType,
                PaymentCurrency = paymentHistory.PaymentCurrency,
                PaymentMethodCode = paymentHistory.PaymentMethodCode,
                Realised = paymentHistory.Realised,
                ReceiptDate = paymentHistory.ReceiptDate != null ? LocalDateTime.FromDateTime(paymentHistory.ReceiptDate.Value) : null,
                PaymentMethodDescription = refPaymentMethodData?.FirstOrDefault(x => x.Code == paymentHistory.PaymentMethodCode)?.Description
            };
        }
    }
}
