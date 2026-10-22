using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    [GraphQLName("PaxOps:Gender")]
    public class Gender
    {
        [GraphQLDescription("Gender code from the document.")]
        public string Code { get; set; }

        public static explicit operator Gender(Infrastructure.Models.CheckInMS.Gender source) => new()
        {
            Code = source.Code
        };
    }
}
