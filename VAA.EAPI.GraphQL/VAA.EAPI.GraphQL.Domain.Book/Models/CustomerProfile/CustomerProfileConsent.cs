namespace VAA.EAPI.GraphQL.Domain.Book.Models.CustomerProfile
{
    public class CustomerProfileConsent
    {
        public string ConsentFor { get; set; }
        public DateTime? ConsentProcessedDate { get; set; }
        public string EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string TermsAndConditionsAccepted { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
    }
}
