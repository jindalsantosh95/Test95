using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus
{
    public class BannerCosmosDocumentEvent : CosmosDocumentEvent
    {
        public BannerCosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType) : base(dto, modType)
        {
            // Required for serialisation
        }

        public BannerCosmosDocumentEvent() : base()
        {
            // Required for serialisation
        }

        public new BannerDTO Document { get; set; }
    }
}
