using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CommonRefDB.Models
{
    public class Product
    {
        public string RowId { get; set; }
        public string ProductCategory { get; set; }
        public string Description { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string DivisionCode { get; set; }
        public string AllowCancellation { get; set; }
        public string ExpiryBasis { get; set; }
        public decimal? ExpiryLeadTime { get; set; }
        public string ExpiryUnitOfMeasure { get; set; }
        public decimal? ExpiryInYears { get; set; }
        public decimal? Duration { get; set; }
        public string AwardPeriodTypeCode { get; set; }
        public string SequenceTypeCode { get; set; }
        public decimal? AwardExpiryPeriodDuration { get; set; }
        public string AwardExpiryPeriodType { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
