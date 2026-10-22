namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Discount
    {
        public string Code { get; set; }
        public Price Amount { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string Pct { get; set; }
    }
}
