using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus
{
    class TargetedMembershipCosmosDocumentEvent : CosmosDocumentEvent
    {
        public TargetedMembershipCosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType) : base(dto, modType)
        {
            // Required for serialisation
        }

        public TargetedMembershipCosmosDocumentEvent() : base()
        {
            // Required for serialisation
        }

        public new TargetedMembershipDTO Document { get; set; }
    }

}