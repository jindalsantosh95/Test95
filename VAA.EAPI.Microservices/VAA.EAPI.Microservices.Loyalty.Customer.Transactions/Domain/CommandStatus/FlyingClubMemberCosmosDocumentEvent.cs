using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus
{
    public class FlyingClubMemberCosmosDocumentEvent : CosmosDocumentEvent
    {
        public FlyingClubMemberCosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType) : base(dto, modType)
        {
            // Required for serialisation
        }

        public FlyingClubMemberCosmosDocumentEvent() : base()
        {
            // Required for serialisation
        }

        public new FlyingClubMemberDTO Document { get; set; }
    }

}
