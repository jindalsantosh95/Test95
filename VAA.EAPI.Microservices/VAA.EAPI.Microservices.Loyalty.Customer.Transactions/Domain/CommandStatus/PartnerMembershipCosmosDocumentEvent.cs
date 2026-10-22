using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus
{
    public class PartnerMembershipCosmosDocumentEvent : CosmosDocumentEvent
    {
        public PartnerMembershipCosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType) : base(dto, modType)
        {
            // Required for serialisation
        }

        public PartnerMembershipCosmosDocumentEvent() : base()
        {
            // Required for serialisation
        }

        public new PartnerMembershipDTO Document { get; set; }
    }

}