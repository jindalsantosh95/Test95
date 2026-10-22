using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    [GraphQLName("TripsBasketFQTDetails")]
    public class FQTMembership
    {
        public string FQTProgramme { get; set; }
        public string FQTNumber { get; set; }
        public string FQTTierCode { get; set; }
    }
}