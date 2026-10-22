using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models
{
    public partial class MediaCodes
    {
        public string RowId { get; set; }
        public string MediaCode { get; set; }
        public string MediaCodeCategory { get; set; }
        public string MediaCodeDescription { get; set; }
        public decimal? Miles { get; set; }
        public string ActivityStatement { get; set; }
        public string MediaChannelCode { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
