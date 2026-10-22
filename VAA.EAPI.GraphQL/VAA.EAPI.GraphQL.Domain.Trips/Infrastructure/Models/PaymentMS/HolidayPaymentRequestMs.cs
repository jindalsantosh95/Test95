namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS
{
    public sealed class HolidayPaymentRequestMs
    {
        public string BookingReference { get; set; }       
        public string Surname { get; set; }       
        public string DateOfBirth { get; set; }
        public decimal? AmountToPay { get; set; }
        public string ReturnUrl { get; set; }
        public string ExpiryUrl { get; set; }
        public string PayPalClientUrl { get; set; }
        public string ContextPath { get; set; }
    }
}
