namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.Baggage
{
    public class SelectedBaggageOffer
    {
        public string PassengerNiN { get; set; }
        public string BaggageOfferId { get; set; }
        public int BaggageCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string CurrencyCode { get; set; }
        public int DecimalPrecisionCount { get; set; }
    }
}
