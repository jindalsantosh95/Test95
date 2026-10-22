using System;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class ContactHistoryDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }

        public string ContactId { get; set; }

        public string ContactSubType { get; set; }

        public string ContactType { get; set; }

        public string ContactValue { get; set; }

        public string CountryCode { get; set; }

        public string CountryWithCode { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? CreatedOn { get; set; }

        public string Description { get; set; }

        public string LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public string Primary { get; set; }

        public string SourceIdentifier { get; set; }

        public string SourceSystem { get; set; }

        public string Usage { get; set; }

        public string Valid { get; set; }

        public string CustomerId { get; set; }

        public string DocType { get; set; }

        public ContactHistoryDTO()
        {

        }

        public static explicit operator ContactHistoryDTO(ContactHistory contactHistory)
        {
            return new ContactHistoryDTO()
            {
                Id = contactHistory.Id,
                SchemaVersion = Constants.Application.SchemaVersion,
                ContactId = contactHistory.ContactId,
                ContactSubType = contactHistory.ContactSubType,
                ContactType = contactHistory.ContactType,
                ContactValue = contactHistory.ContactValue,
                CountryCode = contactHistory.CountryCode,
                CountryWithCode = contactHistory.CountryWithCode,
                CreatedBy = contactHistory.CreatedBy,
                CreatedOn = contactHistory.CreatedOn,
                Description = contactHistory.Description,
                LastUpdatedBy = contactHistory.LastUpdatedBy,
                LastUpdatedOn = contactHistory.LastUpdatedOn,
                Primary = contactHistory.Primary,
                SourceIdentifier = contactHistory.SourceIdentifier,
                SourceSystem = contactHistory.SourceSystem,
                Usage = contactHistory.Usage,
                Valid = contactHistory.Valid,
                CustomerId = contactHistory.CustomerId,
                DocType = DocumentType.ContactHistory.ToString(),
                PartitionKey = contactHistory.CustomerId
            };
        }
    }
}
