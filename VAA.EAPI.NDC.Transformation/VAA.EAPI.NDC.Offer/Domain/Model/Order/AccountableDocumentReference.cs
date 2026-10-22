namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{

    public class AccountableDocumentReference
    {

        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string CheckDigit { get; set; }
        public string CouponNumber { get; set; }
        public string CouponStatus { get; set; }
        public string SegmentID { get; set; }
        public Link DocumentLink { get; set; }
        public string DocumentIssuer { get; set; }
        public LocalAndUTCDateTime IssueDate { get; set; }
        public string CustomerIdentity { get; set; }
        public Link CustomerLink { get; set; }
        public string DateOfIssue { get; set; }
        public string Price { get; set; }
        public string FarebasisFareclassCode { get; set; }
        public string CurrencyCode { get; set; }


    }
}
