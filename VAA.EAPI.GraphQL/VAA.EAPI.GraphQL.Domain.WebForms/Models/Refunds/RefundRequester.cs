namespace VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds
{
    public class RefundRequester
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; } = default;
    }
}
