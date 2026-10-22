using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails
{
    public class TitleDetails
    {
        public string RowId { get; set; }
        public string EnvelopeSalutationTemplate { get; set; }
        public string FormalEnvelopeSalutationTemplate { get; set; }
        public string Gender { get; set; }
        public string InformalSalutationTemplate { get; set; }
        public decimal? Priority { get; set; }
        public string Title { get; set; }
        public string VisibleToSuperUser { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
    }
}
