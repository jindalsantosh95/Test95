namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public sealed class HolidayPaymentRequest
    {
        public string TripResourceId { get; set; }
        public decimal? AmountToPay { get; set; }
        public string ReturnUrl { get; set; }
        public string ExpiryUrl { get; set; }
        public string PayPalClientUrl { get; set; }
        public string ContextPath { get; set; }
    }
}
