using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models
{
    public class PartnerTerms
    {
        public string RowId { get; set; }
        public string ProgramId { get; set; }
        public string SkipFlyingClubAccrualValidations { get; set; }
        public DateTime? AgreementStartDate { get; set; }
        public DateTime? AgreementEndDate { get; set; }
        public decimal? RetroClaimMinimumPeriod { get; set; }
        public string SourceIdentifier { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? DblastUpdatedOn { get; set; }
        public string DblastUpdatedSrc { get; set; }
    }
}
