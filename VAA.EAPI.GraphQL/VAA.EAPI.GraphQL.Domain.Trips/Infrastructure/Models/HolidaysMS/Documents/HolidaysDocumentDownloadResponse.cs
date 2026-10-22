using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents
{
    public class HolidaysDocumentDownloadResponse
    {
        public string DownloadLink { get; set; }
        public int? DocumentId { get; set; }
        public IEnumerable<Error> Errors { get; set; }
    }
}
