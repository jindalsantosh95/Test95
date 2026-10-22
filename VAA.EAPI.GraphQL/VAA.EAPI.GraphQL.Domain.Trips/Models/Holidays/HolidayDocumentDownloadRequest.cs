namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays;

public sealed class HolidayDocumentDownloadRequest
{
    public string? DocumentId { get; set; }
    public string? DocumentToken { get; set; }
}
