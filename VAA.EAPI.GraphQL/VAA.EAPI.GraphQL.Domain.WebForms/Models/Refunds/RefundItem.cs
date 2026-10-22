using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Models;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds
{
    public class RefundItem
    {
        public string PassengerRef { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public DateOnly FlightDate { get; set; }
        public int FlightNumber { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string CouponNumber { get; set; } = string.Empty;
        public string ClaimNumber { get; set; } = string.Empty;

        public static explicit operator RefundItem(RefundRequestItem item)
        {
            return new RefundItem
            {
                PassengerRef = item.PassengerRef,
                PassengerName = item.PassengerName,
                FlightNumber = item.FlightNumber,
                FlightDate = item.FlightDate,
                TicketNumber = item.TicketNumber,
                CouponNumber = item.CouponNumber,
                ClaimNumber = item.ClaimNumber
            };
        }
    }
}
