using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Holidays
{
    public class BookingPayment
    {
        public decimal Deposit { get; set; }
        public decimal CommissionPercentage { get; set; }
        public decimal BalanceDue { get; set; }
        public decimal TotalPriceToBePaid { get; set; }
        public decimal DepositBase { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public bool DepositRealised { get; set; }
        public bool DepositReceived { get; set; }
        public string CurrencyCode { get; set; }
        public IEnumerable<PaymentHistory> PastPayments { get; set; }

        public static explicit operator BookingPayment(JToken source)
        {
            if (source == null) return default;

            return new()
            {
                DepositReceived = source["rate"]?["depositReceived"]?.Value<bool>() ?? false,
                DepositRealised = source?["rate"]?["depositRealised"]?.Value<bool>() ?? false,
                BalanceDue = source["rate"]?["balanceDue"]?.Value<decimal>() ?? 0,
                CommissionPercentage = source["rate"]?["commissionPercentage"]?.Value<decimal>() ?? 0,
                Deposit = source["rate"]?["deposit"]?.Value<decimal>() ?? 0,
                TotalPaidAmount = source["rate"]?["totalPaidAmount"]?.Value<decimal>() ?? 0,
                TotalPriceToBePaid = source["rate"]?["totalPriceToBePaid"]?.Value<decimal>() ?? 0,
                DepositBase = source["rate"]?["depositBase"]?.Value<decimal>() ?? 0,
                CurrencyCode = source["sellingCurrency"]?.Value<string>() ?? null
            };
        }

        public static explicit operator BookingPayment((JToken packageSummary, JToken paymentsHistory) data)
        {
            var (packageSummary, paymentsHistory) = data;

            if (packageSummary == null) return default;

            var bookingPayment = (BookingPayment)packageSummary;

            if (paymentsHistory != null)
            {
                bookingPayment.PastPayments = paymentsHistory.Select(p => (PaymentHistory)p);
            }

            return bookingPayment;
        }
    }
}
