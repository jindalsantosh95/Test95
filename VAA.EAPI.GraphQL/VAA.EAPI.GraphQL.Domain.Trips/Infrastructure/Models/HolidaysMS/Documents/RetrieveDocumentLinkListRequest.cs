
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class RetrieveDocumentLinkListRequest
    {
        public IEnumerable<DocumentLinkRequest> DocumentLinkRequests { get; set; }

        public static explicit operator RetrieveDocumentLinkListRequest(HolidaysDocumentListResponse source)
        {
            if (source?.Documents?.Any() != true) return null;

            return new()
            {
                DocumentLinkRequests = source.Documents.Select(d => (DocumentLinkRequest)d)
            };
        }
    }
}
