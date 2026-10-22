namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS.Documents;

public sealed class RetrieveDocumentDownloadResponse
{
    public string? DocumentId { get; set; }
    public byte[]? Document { get; set; }
}
