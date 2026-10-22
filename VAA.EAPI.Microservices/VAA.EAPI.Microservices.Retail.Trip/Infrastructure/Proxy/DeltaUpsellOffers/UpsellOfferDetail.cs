namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    public class UpsellOfferDetail
    {
        public int Ordinal { get; set; }
        public OfferCodeType OfferType { get; set; }
        public string CurrencyCode { get; set; }
        public string BaseAmt { get; set; }
        public string TotalAmount { get; set; }
        public LoyaltyProgram LoyaltyProgram { get; set; }
    }
}
