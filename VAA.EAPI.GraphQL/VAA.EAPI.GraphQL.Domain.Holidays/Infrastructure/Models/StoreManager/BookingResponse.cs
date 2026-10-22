namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager;

public class BookingResponse
{
    public string Id { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string WindowId { get; set; } = string.Empty;
    public string CustomerRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public string? NotificationStatus { get; set; }
    public string? NotificationMessage { get; set; }
}
