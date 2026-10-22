using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities
{
    public class ContactHistory
    {
        public string Id { get; set; }

        public string SchemaVersion { get; set; }

        public string DocType { get; set; }

        public string PartitionKey { get; set; }

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

        public static explicit operator ContactHistory(Customers contactHistory)
        {
            IEnumerable<Contact> phoneDetails = contactHistory.ContactDetails.Where(x => x.ContactType == Constants.Application.PhoneContactType && x.Primary == "Y");

            return new ContactHistory()
            {
                Id = Guid.NewGuid().ToString(),
                DocType = DocumentType.ContactHistory.ToString(),
                PartitionKey = contactHistory.PartitionKey,
                CustomerId = contactHistory.CustomerId,
                ContactId = phoneDetails?.First().ContactId,
                SourceIdentifier = phoneDetails?.First().SourceIdentifier,
                SourceSystem = phoneDetails?.First().SourceSystem,
                CreatedOn = DateTime.Parse(contactHistory.CreatedOn),
                CreatedBy = phoneDetails?.First().CreatedBy,
                LastUpdatedOn = DateTime.Parse(contactHistory.LastUpdatedOn),
                LastUpdatedBy = phoneDetails?.First().LastUpdatedBy,
                ContactValue = phoneDetails?.First().ContactValue,
                ContactType = phoneDetails?.First().ContactType,
                ContactSubType = phoneDetails?.First().ContactSubType,
                Usage = phoneDetails?.First().Usage,
                Valid = phoneDetails?.First().Valid,
                Description = phoneDetails?.First().Description,
                CountryCode = phoneDetails?.First().CountryCode,
                CountryWithCode = phoneDetails?.First().CountryWithCode,
                Primary = phoneDetails?.First().Primary
            };
        }
    }
}