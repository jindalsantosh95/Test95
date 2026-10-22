using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models
{
    public class CheckInError
    {
        [GraphQLDescription("Machine-readable error code, for example SecondaryDocNonETARequirement.")]
        public string Code { get; set; }

        [GraphQLDescription("User-displayable message suitable for the requirement context.")]
        public string Message { get; set; }

        [GraphQLDescription("Error category when supplied.")]
        public string Category { get; set; }

        [GraphQLDescription("Display code when supplied.")]
        public string DisplayCode { get; set; }

        [GraphQLDescription("User-displayable title when supplied.")]
        public string Title { get; set; }

    }
}
