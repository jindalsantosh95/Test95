
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class DocumentLinkRequest
    {
        public int? DocumentId { get; set; }
        public string BookingId { get; set; }

        public static explicit operator DocumentLinkRequest(Document source)
        {
            if (source == null) return null;

            return new()
            {
                BookingId = Convert.ToString(source.BookingId),
                DocumentId = source.DocumentId
            };
        }
    }
}
