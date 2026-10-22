using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class AdditionalOfferSetProperties
    {
        public bool LowestFare { get; set; }
        public bool SaturdayNightStay { get; set; }
        public bool Fastest { get; set; }
        public IEnumerable<UpgradeCertificateTripStatus> GlobalUpgradeCertificateTripStatus { get; set; }
        public IEnumerable<UpgradeCertificateTripStatus> RegionalUpgradeCertificateTripStatus { get; set; }
        public string SeatReferenceId { get; set; }
        public string OfferSetId { get; set; }
        public IEnumerable<Product> OfferSetBadges { get; set; }
        public DiscountInfo DiscountInfo { get; set; }
        public IEnumerable<DiscountInEligibilityInfo> DiscountInEligibilityList { get; set; }
        public IEnumerable<PromotionInfo> PromotionsInfo { get; set; }
        public Dictionary<string, string> AdditionalDetailsMap { get; set; }
    }
}
