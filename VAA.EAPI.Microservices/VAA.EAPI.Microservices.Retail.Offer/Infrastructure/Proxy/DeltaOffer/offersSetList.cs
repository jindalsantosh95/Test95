using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class OffersSetList
    {
        public string OfferRequestId { get; set; }
        public string OfferRequestEpochTime { get; set; }
        public string OfferResponseId { get; set; }
        public string OfferResponseEpochTime { get; set; }
        public IEnumerable<OffersSet> OffersSets { get; set; }
        public IEnumerable<OffersSet> SelectedOfferSets { get; set; }
        public IEnumerable<OffersSet> RecommendedOfferSets { get; set; }
        public OfferDataList OfferDataList { get; set; }
        public IEnumerable<ErrorResponses> Errors { get; set; }
        public IEnumerable<Warning> Warnings { get; set; }
    }
}
