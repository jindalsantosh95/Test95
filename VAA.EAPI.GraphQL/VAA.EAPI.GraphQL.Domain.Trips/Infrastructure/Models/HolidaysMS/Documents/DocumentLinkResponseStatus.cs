namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class DocumentLinkResponseStatus
    {
        public int HttpStatusCode { get; set; }
        public IEnumerable<DocumentLinkError> Errors { get; set; }
    }
}
