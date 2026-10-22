using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails
{
    public class PartnerStatusCodeDetails
    {
        public string RowId { get; set; }
        public string StatusCode { get; set; }
        public string Description { get; set; }
        public string PartnerId { get; set; }
        public string SourceIdentifier { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}
