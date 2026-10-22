using System;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails
{
    public class MediaCodeDetails
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
        public DateTime? DblastUpdatedOn { get; set; }
        public string DblastUpdatedSrc { get; set; }
    }
}
