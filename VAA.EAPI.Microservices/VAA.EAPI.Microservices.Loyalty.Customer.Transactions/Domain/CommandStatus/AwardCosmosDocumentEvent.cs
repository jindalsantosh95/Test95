using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus
{
    class AwardCosmosDocumentEvent : CosmosDocumentEvent
    {
        public AwardCosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType) : base(dto, modType)
        {
            // Required for serialisation
        }

        public AwardCosmosDocumentEvent() : base()
        {
            // Required for serialisation
        }

        public new AwardDTO Document { get; set; }
    }

}