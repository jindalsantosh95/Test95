using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Nationality")]
    public class Nationality
    {
        [GraphQLDescription("Nationality display name when supplied.")]
        public string Name { get; set; }

        [GraphQLDescription("Nationality country code. Uses ISO alpha-3 codes, with the Germany exception D.")]
        public string Code { get; set; }

        public static explicit operator Nationality(Infrastructure.Models.CheckInMS.Nationality source) => new()
        {
            Name = source.Name,
            Code = source.Code
        };
    }
}
