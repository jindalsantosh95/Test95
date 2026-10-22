namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class DiscountInfo
    {
        public string DiscountTypeCode { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string DiscountPct { get; set; }
        public bool NonDiscountedOffersAvailable { get; set; }
    }
}
