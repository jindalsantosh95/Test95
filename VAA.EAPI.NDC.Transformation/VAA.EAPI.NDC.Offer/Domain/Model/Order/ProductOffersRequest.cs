namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class ProductOffersRequest
    {
        public IATAPayload IATAPayload { get; set; }
        public POS POS { get; set; }
        public NDCDocument NDCDocument { get; set; }
        public NDCParties Parties { get; set; }
        public Customer Customer { get; set; }
        public ProductOffersRequestTravellers Travellers { get; set; }
        public Qualifiers OfferQualifiers { get; set; }
        public Preferences OfferPreferences { get; set; }
        public ProductOffersRequestCoreRequest CoreRequest { get; set; }
        public OrderReshopRequest ReShop { get; set; }
    }
}