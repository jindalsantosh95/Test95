using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Pos")]
    public class Pos
    {
        public string CountryCode { get; set; }
        public string CityCode { get; set; }
    }
}