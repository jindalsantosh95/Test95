using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferPriceCriteria
    {
        public PointOfSale PointOfSale { get; set; }
        public Party Sender { get; set; }
        public IEnumerable<OfferCriteria> OffersCriteria { get; set; }
    }
}
