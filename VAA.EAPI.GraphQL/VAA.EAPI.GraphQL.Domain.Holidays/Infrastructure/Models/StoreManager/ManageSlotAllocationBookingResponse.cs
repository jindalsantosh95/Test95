namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager;

public class ManageSlotAllocationBookingResponse
{
    public string BookingId { get; set; } = string.Empty;
    public string CustomerRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

