using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class ContactHistory
    {
        public string ContactId { get; set; }
        public string ContactSubType { get; set; }
        public string ContactType { get; set; }
        public string ContactValue { get; set; }
        public string CountryCode { get; set; }
        public string CountryWithCode { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string Primary { get; set; }
        public string Description { get; set; }
        public string Usage { get; set; }
        public string Valid { get; set; }

        public static explicit operator ContactHistory(Contact source)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return new ContactHistory
            {
                ContactId = source.ContactId,
                ContactSubType = source.ContactSubType,
                ContactType = source.ContactType,
                ContactValue = source.ContactValue,
                CountryCode = source.CountryCode,
                CountryWithCode = source.CountryWithCode,
                LastUpdatedBy = source.LastUpdatedBy,
                LastUpdatedOn = DateTime.Parse(source.LastUpdatedOn),
                Primary = source.Primary,
                Description = source.Description,
                Usage = source.Usage,
                Valid = source.Valid
            };
        }
    }
}
