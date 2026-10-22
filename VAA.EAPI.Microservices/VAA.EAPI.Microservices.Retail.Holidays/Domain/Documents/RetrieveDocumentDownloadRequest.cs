namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;

public sealed class RetrieveDocumentDownloadRequest
{
    public string? BookingId { get; set; }
    public string? DocumentId { get; set; }
    public string? DocumentUrl { get; set; }
}
