using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.OrderDataMicroservice.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Infrastructure.RefundMicroservice.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Interfaces;
using VAA.EAPI.GraphQL.Domain.WebForms.Models.Refunds;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Application
{
    public class RefundRequestService : IRefundRequestService
    {
        private readonly IRefundProvider _refundProvider;
        private readonly IBookingProvider _bookingProvider;

        public RefundRequestService(IRefundProvider refundProvider, IBookingProvider bookingProvider)
        {
            _refundProvider = refundProvider ?? throw new ArgumentNullException(nameof(refundProvider));
            _bookingProvider = bookingProvider ?? throw new ArgumentNullException(nameof(bookingProvider));
        }

        public async Task<RefundForm> FindMyBooking(RefundForm requestForm)
        {
            // Validate all form fields are provided
            ValidateRefundForm(requestForm);

            // Go find the booking details and update the request form with the pax / flight / ticket data which will be used to check for duplicates in CRM and to submit the refund request to CRM.
            requestForm = await _bookingProvider.FindMyBooking(requestForm);


            // Check for duplicates in CRM based on the pax / flight / ticket data. This is to prevent multiple refund requests being submitted for the same booking which could lead to multiple claims being generated in CRM for the same booking.
            if (requestForm.Entries != null && requestForm.Entries.Any())
            {
                requestForm = await _refundProvider.CheckForDuplicates(requestForm);
            }

            return requestForm;
        }

        public async Task<string> SubmitRefundRequest(RefundForm requestForm)
        {
            ValidateRefundForm(requestForm);
            if (requestForm.Entries == null || !requestForm.Entries.Any())
            {
                throw new ArgumentException("At least one refund entry is required.", nameof(requestForm.Entries));
            }
            if (requestForm.Entries.Any(e => !e.ClaimNumber.IsNullOrWhiteSpace()))
            {
                throw new InvalidOperationException("One or more entries have already been submitted for refund. Please remove duplicate entries before submitting the refund request.");
            }

            return await _refundProvider.SubmitRefundRequest(requestForm);
        }

        private void ValidateRefundForm(RefundForm form)
        {
            if (form == null)
            {
                throw new ArgumentNullException(nameof(form));
            }
            if (form.Requester == null)
            {
                throw new ArgumentNullException(nameof(form.Requester), "Requester information is required.");
            }
            if (string.IsNullOrWhiteSpace(form.Requester.FirstName))
            {
                throw new ArgumentException("Requester's first name is required.", nameof(form.Requester.FirstName));
            }
            if (string.IsNullOrWhiteSpace(form.Requester.LastName))
            {
                throw new ArgumentException("Requester's last name is required.", nameof(form.Requester.LastName));
            }
            if (string.IsNullOrWhiteSpace(form.Requester.BookingReference))
            {
                throw new ArgumentException("Booking reference is required.", nameof(form.Requester.BookingReference));
            }
            if (string.IsNullOrWhiteSpace(form.Requester.Email))
            {
                throw new ArgumentException("Requester's email is required.", nameof(form.Requester.Email));
            }
            if (form.Requester.BookingDate == default)
            {
                throw new ArgumentException("Booking date must be provided.", nameof(form.Requester.BookingDate));
            }
        }
    }
}
