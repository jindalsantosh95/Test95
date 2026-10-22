using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class BagOfferCriteria
    {
        public string PassengerId { get; set; }
        public string TripId { get; set; }
        public IEnumerable<string> FlightSegmentIds { get; set; }
        public string BagSequenceNum { get; set; }
        public string BagTypeCode { get; set; }
        public bool CarryOnIndicator { get; set; }
        public BagSpecification BagSpecification { get; set; }
        public Dictionary<string, string> AdditionalCriteriaMap { get; set; }
    }
}
