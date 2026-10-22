using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:CountryOfResidence")]
    public class CountryOfResidence
    {
        [GraphQLDescription("Country of residence display name when supplied.")]
        public string Name { get; set; }

        [GraphQLDescription("Country-of-residence code. Uses ISO alpha-3 codes, with the Germany exception D.")]
        public string Code { get; set; }

        public static explicit operator CountryOfResidence(Infrastructure.Models.CheckInMS.CountryOfResidence source) => new()
        {
            Name = source.Name,
            Code = source.Code
        };
    }
}
