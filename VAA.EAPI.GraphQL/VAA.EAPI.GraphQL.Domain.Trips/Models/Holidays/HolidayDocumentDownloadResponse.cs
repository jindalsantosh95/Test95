namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;

public sealed class HolidayDocumentDownloadResponse
{
    public string? DocumentId { get; set; }
    public byte[]? Document { get; set; }
}
