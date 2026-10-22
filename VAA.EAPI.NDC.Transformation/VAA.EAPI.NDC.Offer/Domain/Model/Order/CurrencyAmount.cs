namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class CurrencyAmount
    {
        public string MonetaryAmount { get; set; }
        public string OfferCurrency { get; set; }
        public float? ForexRate { get; set; }
        public string BaseCurrency { get; set; }
        public decimal? DecimalPrecisionCount { get; set; }

    }
}
