using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.TripMS
{
    [GraphQLName("TripsMSPhone")]
    public class Phone
    {
        public string CountryCode { get; set; }
        public string Number { get; set; }
    }
}
