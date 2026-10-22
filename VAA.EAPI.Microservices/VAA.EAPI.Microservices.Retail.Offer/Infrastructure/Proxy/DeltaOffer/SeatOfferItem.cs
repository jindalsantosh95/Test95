namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatOfferItem
    {
        public string OfferId { get; set; }
        public string OfferItemId { get; set; }
        public OfferSupplier OfferItemSupplierGeneratedId { get; set; }
        public string BrandId { get; set; }
        public ContentUrl BrandIdUrl { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductGroupName { get; set; }
        public string ReasonForIssuanceCode { get; set; }
        public string ReasonForIssuanceSubCode { get; set; }
        public Price PriceAmt { get; set; }
        public Price BasePrice { get; set; }
        public Price Tax { get; set; }
        public string CustomerId { get; set; }
        public string FlightSegmentId { get; set; }
        public string FlightLegId { get; set; }
    }
}
