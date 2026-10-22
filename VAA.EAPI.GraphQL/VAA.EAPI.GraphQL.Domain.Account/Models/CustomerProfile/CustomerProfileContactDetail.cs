namespace VAA.EAPI.GraphQL.Domain.Account.Models.CustomerProfile
{
    public class CustomerProfileContactDetail
    {
        public string ContactId { get; set; }
        public string ContactSubType { get; set; }
        public string ContactType { get; set; }
        public string ContactValue { get; set; }
        public string CountryCode { get; set; }
        public string CountryWithCode { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string Primary { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public string Usage { get; set; }
        public string Valid { get; set; }
    }
}
