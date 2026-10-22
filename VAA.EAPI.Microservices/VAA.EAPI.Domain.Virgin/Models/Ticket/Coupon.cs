using Newtonsoft.Json.Linq;
using System;
namespace VAA.EAPI.Domain.Virgin.Models.Ticket
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
        public string TicketExpirationDate { get; set; }
        public string TicketEffectiveDate { get; set; }

        public static explicit operator Coupon(JObject source)
        {
            return new Coupon()
            {
                SegmentRefId = source["SegmentRefId"]?.ToString(),
                CouponNumber = source["CouponNumber"]?.ToString(),
                TicketNumber = source["TicketNumber"]?.ToString(),
                CheckDigit = source["CheckDigit"]?.ToString(),
                CouponStatus = source["CouponStatus"]?.ToString(),
                TravelDate = source["TravelDate"] != null ? (DateTime)source["TravelDate"] : null,
                Designator = source["Designator"]?.ToString(),
                FareBasis = source["FareBasis"]?.ToString(),
                ConnectionTicketNumber = source["ConnectionTicketNumber"]?.ToString(),
                TicketExpirationDate = source["TicketExpirationDate"]?.ToString(),
                TicketEffectiveDate = source["TicketEffectiveDate"]?.ToString()
            };
        }

    }

}