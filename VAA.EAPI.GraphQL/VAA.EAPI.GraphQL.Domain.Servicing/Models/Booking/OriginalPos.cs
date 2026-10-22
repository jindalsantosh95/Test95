using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.Booking
{
    public class OriginalPos
    {
        [GraphQLDescription("Agency Id")]
        public string? AgencyId { get; set; }
        [GraphQLDescription("Agent Id")]
        public string? AgentId { get; set; }
    }
}
