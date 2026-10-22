namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class OfferItem
    {
        public string offerItemId { get; set; }
        public string offerItemDesc { get; set; }
        public OfferSupplier offerSupplier { get; set; }
        public DateTime offerItemExpirationTimeLimitUtcTs { get; set; }
        public List<OfferItemPricing> offerItemPricing { get; set; }
        public bool mandatory { get; set; }
        public List<string> tripIds { get; set; }
        public List<string> flightSegmentIds { get; set; }
        public List<object> passengerIds { get; set; }
        public List<RetailItem> retailItems { get; set; }
    }
}
