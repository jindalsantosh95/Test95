using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OfferToComplement
    {
        public string OfferId { get; set; }
        public IEnumerable<string> OfferItemIdList { get; set; }
    }
}
