using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Retailing.Infrastructure.Models.OrderSearch
{
    [GraphQLName("Retailing.Service")]
    public class Service
    {
        public string ServiceId { get; set; }
        public ProductSegmentAssociation ProductSegmentAssociation { get; set; }
        public IEnumerable<string> ServiceBeneficiaries { get; set; }
        public IEnumerable<string> OrderInstructionRefs { get; set; }

    }
}