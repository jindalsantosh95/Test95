namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager;

public class ManageSlotAllocationItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string WindowId { get; set; } = string.Empty;
    public string SlotId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public int AllocationIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? BookingId { get; set; }
    public string? CustomerRef { get; set; }
}
