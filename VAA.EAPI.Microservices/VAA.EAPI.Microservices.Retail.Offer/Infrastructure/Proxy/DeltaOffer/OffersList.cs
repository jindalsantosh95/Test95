using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OffersList
    {
        public string OfferRequestId { get; set; }
        public string OfferResponseId { get; set; }
        public IEnumerable<Offer> Offers { get; set; }
        public OfferDataList OfferDataList { get; set; }
        public Link Link { get; set; }
        public IEnumerable<AdditionalOfferSetProperties> AdditionalOfferSetProperties { get; set; }
    }
}
