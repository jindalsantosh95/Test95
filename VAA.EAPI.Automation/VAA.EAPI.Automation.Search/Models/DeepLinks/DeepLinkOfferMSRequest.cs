using CustomerDetails = VAA.EAPI.Automation.Search.Models.OfferSearch.CustomerDetails;
using Pos = VAA.EAPI.Automation.Search.Models.OfferSearch.Pos;

namespace VAA.EAPI.Automation.Search.Models.DeepLinks
{
    public class DeepLinkOfferMSRequest
    {
        public string metaPartnerId { get; set; }
        public Pos pos { get; set; }
        public List<CustomerDetails> customerDetails { get; set; }
        public LoyaltyMember loyaltyMember { get; set; }
        public List<FlightSegment> flightSegments { get; set; }
        public string currentTripIndexId { get; set; }
        public string dominantSegmentBrandId { get; set; }
        public TotalOfferedAmount totalOfferedAmount { get; set; }
    }
}