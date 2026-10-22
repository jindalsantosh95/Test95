using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.ContactNotes;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class ContactNoteDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string Brand { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Description { get; set; }
        public string NoteTypeGroup { get; set; }
        public string NoteType { get; set; }
        public string NoteId { get; set; }

        public ContactNoteDTO()
        {
            // Required for serialisation
        }


        public ContactNoteDTO(ContactNote contactNote)
        {
            SchemaVersion = Constants.Application.ContactNoteDocSchemaVersion;
            Brand = Constants.Application.ContactNoteBrand;
            DocType = ConfigReader.Get(EnvironmentSettings.ContactNoteDocType);

            Id = contactNote.Id;
            PartitionKey = contactNote.CustomerId;
            CustomerId = contactNote.CustomerId;
            CreatedOn = contactNote.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            CreatedBy = contactNote.CreatedBy;
            LastUpdatedOn = contactNote.LastUpdatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat);
            LastUpdatedBy = contactNote.LastUpdatedBy;

            SourceIdentifier = string.Empty;
            SourceSystem = string.Empty;

            Description = contactNote.Text;
            NoteTypeGroup = contactNote.Group;
            NoteType = contactNote.Type;
            NoteId = contactNote.Id;
        }

        public ContactNote ToContactNote()
        {
            return new ContactNote(this);
        }
    }
}

