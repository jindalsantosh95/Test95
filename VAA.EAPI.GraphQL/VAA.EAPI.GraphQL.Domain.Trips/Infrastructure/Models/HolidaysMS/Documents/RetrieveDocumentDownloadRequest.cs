namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents;

public sealed class RetrieveDocumentDownloadRequest
{
    public string? BookingId { get; set; }
    public string? DocumentId { get; set; }
    public string? DocumentUrl { get; set; }
}
