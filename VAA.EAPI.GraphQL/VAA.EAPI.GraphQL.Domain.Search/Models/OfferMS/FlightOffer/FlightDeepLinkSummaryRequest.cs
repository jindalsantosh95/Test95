using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlightOffer
{
    public class FlightDeepLinkSummaryRequest
    {
        public string MetaPartnerId { get; set; }
        public Pos Pos { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public LoyaltyMemberDetails LoyaltyMember { get; set; }
        public IEnumerable<DeepLinkSegment> FlightSegments { get; set; }
        public string CurrentTripIndexId { get; set; }
        public string PromoCode { get; set; }
        public string DominantSegmentBrandId { get; set; }
        public Amount TotalOfferedAmount { get; set; }
    }
}
