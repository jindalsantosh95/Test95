using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Seats
{
    [GraphQLName("TripsSeatMareketingInfo")]
    public class MarketingInfo
    {
        public string DescText { get; set; }
        public string Url { get; set; }
    }
}