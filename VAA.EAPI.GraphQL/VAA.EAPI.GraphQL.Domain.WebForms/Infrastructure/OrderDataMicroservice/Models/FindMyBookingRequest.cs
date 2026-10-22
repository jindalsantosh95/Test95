using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Models
{
    public class FindMyBookingRequest
    {
        public string FirstName { get; internal set; }
        public string LastName { get; internal set; }
        public string BookingReference { get; internal set; }
        public string Email { get; internal set; }
        public DateOnly BookingDate { get; internal set; }

        public static explicit operator FindMyBookingRequest(RefundForm form)
        {
            return new FindMyBookingRequest
            {
                FirstName = form.Requester.FirstName,
                LastName = form.Requester.LastName,
                BookingReference = form.Requester.BookingReference,
                Email = form.Requester.Email,
                BookingDate = form.Requester.BookingDate
            };
        }
    }
}
