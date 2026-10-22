
namespace VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity
{
    public class Coupon
    {
        public string DeparturePoint { get; set; }
        public string ArrivalPoint { get; set; }
        public string LastNameNumber { get; set; }
        public string FirstNameNumber { get; set; }
        public string TravelDate { get; set; }
        public string TicketNumber { get; set; }
        public string CouponStatus { get; set; }
        public string CouponNumber { get; set; }
        public string CheckDigit { get; set; }
    }
}
