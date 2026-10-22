namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public sealed class HolidayPaymentResponseMs
    {
        public decimal AmountToPay { get; set; }
        public string Status { get; set; }
        public HolidayPaymentAmountMs PaymentAmount { get; set; }
        public string PaymentLink { get; set; }
        public string RemoveLink { get; set; }
    }
}
