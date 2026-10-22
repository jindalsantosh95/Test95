using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class Offer
    {
        public string OfferId { get; set; }
        public int OfferSequenceId { get; set; }
        public OfferResponsibleOwner Owner { get; set; }
        public OfferToComplement OfferToComplement { get; set; }
        public string RecordLocatorId { get; set; }
        public OrderToComplement OrderToComplement { get; set; }
        public IEnumerable<Pricing> OfferPricing { get; set; }
        public string PaymentTimeLimitUtcTs { get; set; }
        public bool Selected { get; set; }
        public bool AddonOffer { get; set; }
        public bool UpsellOffer { get; set; }
        public bool SoldOut { get; set; }
        public string PriceGuaranteeTimeLimitUtcTs { get; set; }
        public string OfferExpirationTimeLimitUtcTs { get; set; }
        public IEnumerable<OfferItem> OfferItems { get; set; }
        public TripsCertificateEligibilityInfo TripsCertificateEligibilityInfo { get; set; }
        public IEnumerable<Link> Links { get; set; }
        public Dictionary<string, string> AdditionalOfferDetailsMap { get; set; }
        public AdditionalOfferProperties AdditionalOfferProperties { get; set; }
    }
}
