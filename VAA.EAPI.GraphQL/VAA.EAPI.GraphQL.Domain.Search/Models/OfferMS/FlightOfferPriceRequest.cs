using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS
{
    public class FlightOfferPriceRequest
    {
        public Pos? Pos { get; set; }
        public Parties? Parties { get; set; }
        public FlightSearchRequest? FlightSearchRequest { get; set; }
        public LoyaltyMemberDetails? LoyaltyMember { get; set; }
        public IEnumerable<Customer>? CustomerDetails { get; set; }
    }
}