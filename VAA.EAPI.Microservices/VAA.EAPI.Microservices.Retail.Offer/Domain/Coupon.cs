using System;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain
{
    public class Coupon
    {
        public string segmentRefId { get; set; }
        public string CouponNumber { get; set; }
        public string TicketNumber { get; set; }
        public string CheckDigit { get; set; }
        public string CouponStatus { get; set; }
        public DateTime? TravelDate { get; set; }
        public string Designator { get; set; }
        public string FareBasis { get; set; }
    }

}