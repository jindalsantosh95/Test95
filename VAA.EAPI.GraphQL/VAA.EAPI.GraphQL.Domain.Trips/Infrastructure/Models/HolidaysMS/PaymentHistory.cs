using VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS
{
    public class PaymentHistory
    {
        public decimal? CurrencyAmount { get; set; }
        public DateTime? RealisedDate { get; set; }
        public string PaymentCurrency { get; set; }
        public string PaymentMethodCode { get; set; }
        public bool Realised { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public PaymentTransactionType TransactionType { get; set; }
    }
}
