namespace VAA.EAPI.GraphQL.Domain.Book.Models.Ticket
{
    public class Coupon
    {
        public string SegmentRefId { get; set; }
        public string CouponNumber { get; set; }
        public string TicketNumber { get; set; }
        public string CheckDigit { get; set; }
        public string CouponStatus { get; set; }
        public DateTime? TravelDate { get; set; }
        public string Designator { get; set; }
        public string FareBasis { get; set; }
        public string ConnectionTicketNumber { get; set; }

    }

}