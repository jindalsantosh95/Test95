namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager;

public class ManageBookingResponse
{
    public string Id { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public string StoreSlotId { get; set; } = string.Empty;
    public string CustomerRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset WindowStartAt { get; set; }
    public DateTimeOffset WindowEndAt { get; set; }
    public int WindowCapacity { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

