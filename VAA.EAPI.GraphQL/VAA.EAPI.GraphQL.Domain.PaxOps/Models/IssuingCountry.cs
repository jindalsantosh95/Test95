using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    public class IssuingCountry
    {
        [GraphQLDescription("Passport/document issuing country code. Uses ISO alpha-3 codes, with the Germany exception D.")]
        public string Code { get; set; }

        public static explicit operator IssuingCountry(Infrastructure.Models.CheckInMS.IssuingCountry source) => new()
        {
            Code = source.Code
        };
    }
}
