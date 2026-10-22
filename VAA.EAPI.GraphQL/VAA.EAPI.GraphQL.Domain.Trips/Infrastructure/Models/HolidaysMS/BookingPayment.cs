namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
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

    }
}
