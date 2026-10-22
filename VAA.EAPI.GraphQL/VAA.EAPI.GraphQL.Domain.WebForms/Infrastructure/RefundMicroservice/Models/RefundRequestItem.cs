namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Models
{
    public class RefundRequestItem
    {
        public string PassengerRef { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public DateOnly FlightDate { get; set; }
        public int FlightNumber { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string CouponNumber { get; set; } = string.Empty;
        public string ClaimNumber { get; set; } = string.Empty;

        public static explicit operator RefundRequestItem(VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds.RefundItem item)
        {
            return new RefundRequestItem
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
