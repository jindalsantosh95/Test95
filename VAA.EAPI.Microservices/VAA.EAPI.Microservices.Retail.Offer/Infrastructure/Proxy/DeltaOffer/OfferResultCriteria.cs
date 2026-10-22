namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferResultCriteria
    {
        public bool ExcludeCheckedBagAllowances { get; set; }
        public bool ExcludeCarryOnBagAllowances { get; set; }
        public bool ExcludeSpecialityBagAllowances { get; set; }
        public bool ExcludePenalties { get; set; }
        public bool ExcludeTaxBreakDown { get; set; }
        public bool ExcludeAmenities { get; set; }
    }
}
