namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class SeatPrice
    {
        public string MonetaryAmount { get; set; }
        public string OfferCurrency { get; set; }
        public float? ForexRate { get; set; }
        public string BaseCurrency { get; set; }
        public string DecimalPrecisionCount { get; set; }               
    }
}
