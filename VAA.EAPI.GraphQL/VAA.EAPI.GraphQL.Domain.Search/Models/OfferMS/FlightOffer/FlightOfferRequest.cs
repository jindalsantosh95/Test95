using VAA.EAPI.GraphQL.Domain.Search.Models.FlightOffer;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferMS.FlightOffer
{
    public class FlightOfferRequest
    {
        public Pos? Pos { get; set; }
        public Parties? Parties { get; set; }
        public FlightSearchRequest? FlightSearchRequest { get; set; }
        public IEnumerable<Customer>? CustomerDetails { get; set; }
        public LoyaltyMemberDetails? LoyaltyMember { get; set; }

        public static explicit operator FlightOfferRequest(FlightOfferBaseRequest source)
        {
            return new FlightOfferRequest
            {
                Pos = source.Pos,
                Parties = source.Parties,
                FlightSearchRequest = source.FlightSearchRequest,
                CustomerDetails = source.CustomerDetails,
                LoyaltyMember = source.LoyaltyMember
            };
        }
    }
}