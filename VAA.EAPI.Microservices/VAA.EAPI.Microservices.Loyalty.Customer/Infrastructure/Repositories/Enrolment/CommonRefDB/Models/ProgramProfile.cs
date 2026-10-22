using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models
{
    public partial class ProgramProfile
    {
        public string RowId { get; set; }
        public string Brand { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string Active { get; set; }
        public string ApplyType { get; set; }
        public string ProgramDescription { get; set; }
        public DateTime? EndDate { get; set; }
        public string PartnerId { get; set; }
        public string ProgramName { get; set; }
        public string PartnerFlag { get; set; }
        public string PointsToPayFlag { get; set; }
        public string PointExpiryLevel { get; set; }
        public DateTime? StartDate { get; set; }
        public string ProgramCode { get; set; }
    }
}
