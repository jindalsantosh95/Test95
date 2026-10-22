using VAA.EAPI.NDC.Order.Constants;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS
{
    public class AccountableDocumentReference
    {
        public DocumentTypeEnum? DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string CheckDigit { get; set; }
        public string CouponNumber { get; set; }
        public string CouponStatus { get; set; }
        public string SegmentRefId { get; set; }
        public string DocumentIssuer { get; set; }
        public string IssueDate { get; set; }
        public string CustomerRefId { get; set; }
        public string CurrentSeatAssignment { get; set; }
    }
}
