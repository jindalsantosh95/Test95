namespace VAA.EAPI.Automation.Core.Delta.Models.OfferSearch.DeltaMSResponse
{
    public class Offer
    {
        public string offerId { get; set; }
        public int offerSequenceId { get; set; }
        public List<OfferPricing> offerPricing { get; set; }
        public bool selected { get; set; }
        public bool addonOffer { get; set; }
        public bool upsellOffer { get; set; }
        public bool soldOut { get; set; }
        public DateTime offerExpirationTimeLimitUtcTs { get; set; }
        public List<OfferItem> offerItems { get; set; }
        public AdditionalOfferProperties additionalOfferProperties { get; set; }
    }
}
