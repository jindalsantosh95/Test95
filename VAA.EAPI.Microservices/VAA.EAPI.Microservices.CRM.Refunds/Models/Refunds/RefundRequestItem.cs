namespace VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds;

public class RefundRequestItem
{
    public string PassengerRef { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public DateOnly FlightDate { get; set; }
    public int FlightNumber { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string CouponNumber { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
}
