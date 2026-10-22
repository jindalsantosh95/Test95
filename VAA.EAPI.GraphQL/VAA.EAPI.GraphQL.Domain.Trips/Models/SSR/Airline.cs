
using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.SSR
{
    [GraphQLName("GraphqlSSRAirline")]
    public class Airline
    {
        public string? Code { get; set; }
        public string? Name { get; set; }

        public static explicit operator Airline(Infrastructure.Models.SSRMS.Airline source)
        {
            if (source == null)
                return null;

            return new()
            {
                Code = source.Code,
                Name = source.Name
            };
        }
    }
}