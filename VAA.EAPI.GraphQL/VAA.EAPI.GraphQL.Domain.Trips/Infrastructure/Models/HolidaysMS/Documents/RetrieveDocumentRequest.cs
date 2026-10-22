
using VAA.EAPI.GraphQL.Common.Extensions;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class RetrieveDocumentRequest
    {
        public string BookingReference { get; set; }

        public static explicit operator RetrieveDocumentRequest(string bookingReference)
        {
            if (bookingReference.IsNullOrEmpty()) return null;

            return new()
            {
                BookingReference = bookingReference
            };
        }
    }
}
