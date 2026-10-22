using VAA.EAPI.Infrastructure.Entities.Enums;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus
{
    class ContactNoteCosmosDocumentEvent : CosmosDocumentEvent
    {
        public ContactNoteCosmosDocumentEvent(CosmosDocument dto, CosmosModificationType modType) : base(dto, modType)
        {
            // Required for serialisation
        }

        public ContactNoteCosmosDocumentEvent() : base()
        {
            // Required for serialisation
        }

        public new ContactNoteDTO Document { get; set; }
    }

}