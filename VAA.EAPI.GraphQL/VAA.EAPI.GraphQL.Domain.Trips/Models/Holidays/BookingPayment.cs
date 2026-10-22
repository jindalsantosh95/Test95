using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public class BookingPayment
    {
        public decimal? Deposit { get; set; }
        public decimal? CommissionPercentage { get; set; }
        public decimal? BalanceDue { get; set; }
        public decimal? TotalPriceToBePaid { get; set; }
        public decimal? DepositBase { get; set; }
        public decimal? TotalPaidAmount { get; set; }
        public bool? DepositRealised { get; set; }
        public bool DepositReceived { get; set; }
        public string? CurrencyCode { get; set; }
        public int? PaidPercentage { get; set; }
        public DateOnly? DueDate { get; set; }
        public IEnumerable<PaymentHistory>? PastPayments { get; set; }
        public decimal? PaymentMinAmount { get; set; } = 0m;
        public decimal? PaymentMaxAmount { get; set; }

        public static explicit operator BookingPayment((Infrastructure.Models.HolidaysMS.BookingPayment Payment, IEnumerable<PaymentMethod> refPaymentMethodData, PackageDetails package) source)
        {
            var (payment, refPaymentMethodData, package) = source;

            if (payment == null) return default;

            var paidPercentage = payment.TotalPriceToBePaid > 0 ? (int)Math.Round(((payment.TotalPriceToBePaid - payment.BalanceDue) / payment.TotalPriceToBePaid) * 100m, 0, MidpointRounding.AwayFromZero) : 0;
            return new()
            {
                Deposit = payment.Deposit,
                CommissionPercentage = payment.CommissionPercentage,
                BalanceDue = payment.BalanceDue,
                TotalPriceToBePaid = payment.TotalPriceToBePaid,
                DepositBase = payment.DepositBase,
                TotalPaidAmount = payment.TotalPaidAmount,
                DepositRealised = payment.DepositRealised,
                DepositReceived = payment.DepositReceived,
                CurrencyCode = payment.CurrencyCode ?? Constants.Application.DefaultCurrencyCode,
                PaidPercentage = Math.Clamp(paidPercentage, 0, 100),
                DueDate = package?.BalanceDueDate is { } balanceDueDate && balanceDueDate != default ? DateOnly.FromDateTime((DateTime)(package.BalanceDueDate?.Date)) : null,
                PastPayments = (payment.PastPayments?.Select(p => (PaymentHistory)(p, refPaymentMethodData)))?.OrderByDescending(z => z.RealisedDate),
                PaymentMaxAmount = decimal.Clamp(payment.BalanceDue, 0m, ConfigReader.Try<decimal>(Common.Constants.Environment.HolidayMaxPaymentAmountInGBP, 50_000m))
            };
        }
    }
}
