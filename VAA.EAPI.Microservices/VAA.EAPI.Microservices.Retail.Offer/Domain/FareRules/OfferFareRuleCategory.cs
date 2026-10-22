using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.FareRules
{
    public class OfferFareRuleCategory
    {
        public string OfferRuleCategoryId { get; set; }
        public string OfferRuleCateogryText { get; set; }
        public string OfferRuleCateogryDesc { get; set; }


        public static explicit operator OfferFareRuleCategory(OfferRuleCategory source)
        {
            if (source == null)
                return null;

            return new()
            {
                OfferRuleCategoryId = source.OfferRuleCategoryId,
                OfferRuleCateogryDesc = source.OfferRuleCateogryDesc,
                OfferRuleCateogryText = source.OfferRuleCateogryText
            };
        }
    }
}
