
namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class Consent
    {
        public string ConsentFor { get; set; }

        public string ConsentProcessedDate { get; set; }

        public string EffectiveFrom { get; set; }

        public string EffectiveTo { get; set; }

        public string LastUpdatedBy { get; set; }

        public string LastUpdatedOn { get; set; }

        public string TermsAndConditionsAccepted { get; set; }

        public string CreatedOn { get; set; }

        public string CreatedBy { get; set; }
    }
}
