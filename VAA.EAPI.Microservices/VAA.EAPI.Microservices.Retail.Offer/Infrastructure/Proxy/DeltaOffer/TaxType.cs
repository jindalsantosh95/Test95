namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class TaxType
    {
        public Price TaxAmt { get; set; }
        public string TaxCode { get; set; }
        public string TaxChargeTypeCode { get; set; }
        public Price CostDifferenceAmt { get; set; }
    }
}
