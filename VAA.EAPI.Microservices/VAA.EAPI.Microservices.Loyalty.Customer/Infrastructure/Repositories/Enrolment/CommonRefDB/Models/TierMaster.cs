using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models
{
    public partial class TierMaster
    {
        public string RowId { get; set; }
        public string Name { get; set; }
        public string ExpirationSnapTo { get; set; }
        public decimal? ExpireExtensionDays { get; set; }
        public decimal? NonQualifyingPointsExpirationLength { get; set; }
        public string NonQualifyingPointsExpirationUoM { get; set; }
        public decimal? QualifyingPointsExpirationUoM { get; set; }
        public string QualifyingPointsExpirationLength { get; set; }
        public decimal? QualifyingPeriodLength { get; set; }
        public string QualifyingPeriodUoM { get; set; }
        public decimal? RequalifyingPeriodLength { get; set; }
        public string RequalifyingPeriodUoM { get; set; }
        public decimal Sequence { get; set; }
        public string StatementFrequency { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public string EffectiveTo { get; set; }
    }
}
