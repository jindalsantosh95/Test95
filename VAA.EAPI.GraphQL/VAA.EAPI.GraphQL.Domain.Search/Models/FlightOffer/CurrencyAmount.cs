namespace VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer
{
    public class CurrencyAmount
    {
        public string MonetaryAmount { get; set; }
        public string BaseCurrency { get; set; }
        public int DecimalPrecisionCount { get; set; }
        public decimal PriceDifference { get; set; }
        public string? PriceDifferenceSign { get; set; }
    }
}
