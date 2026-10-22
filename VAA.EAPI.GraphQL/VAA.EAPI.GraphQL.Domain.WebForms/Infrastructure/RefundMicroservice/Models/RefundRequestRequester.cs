namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Models
{
    public class RefundRequestRequester
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string BookingReference { get; set; } = string.Empty;
        public DateOnly BookingDate { get; set; } = default;

        public static explicit operator RefundRequestRequester(VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds.RefundRequester requester)
        {
            return new RefundRequestRequester
            {
                FirstName = requester.FirstName,
                LastName = requester.LastName,
                BookingReference = requester.BookingReference,
                Email = requester.Email,
                BookingDate = requester.BookingDate,
                Reason = requester.Reason,
            };
        }
    }
}
