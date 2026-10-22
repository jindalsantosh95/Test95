namespace VAA.EAPI.Microservices.CRM.Refunds.Models.Refunds
{
    public class RefundRequestRequester
    {
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; }
    }
}
