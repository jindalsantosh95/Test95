namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;

public sealed class RetrieveDocumentDownloadResponse
{
    public string? DocumentId { get; set; }
    public byte[]? Document { get; set; }
    public string? ContentType { get; set; }
    public string? FileName { get; set; }
}
