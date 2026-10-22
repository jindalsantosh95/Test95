namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class Coupon
    {
        public string CheckDigit { get; set; }
        public string CouponNumber { get; set; }
        public string CouponStatus { get; set; }
        public string SegmentRefId { get; set; }
        public string FareBasis { get; set; }
        public string Designator { get; set; }
        public string TicketExpirationDate { get; set; }
    }
}
