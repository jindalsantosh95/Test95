using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Models.TravelRequirements
{
    public class TravelRequirementsRequest
    {
        [GraphQLNonNullType]
        public string PnrLocator { get; set; }

        [GraphQLNonNullType]
        public string GivenNames { get; set; }

        [GraphQLNonNullType]
        public string Surname { get; set; }
    }
}
