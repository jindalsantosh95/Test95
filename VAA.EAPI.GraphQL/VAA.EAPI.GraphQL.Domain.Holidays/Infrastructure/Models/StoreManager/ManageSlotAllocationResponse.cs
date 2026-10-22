namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager;

public class ManageSlotAllocationResponse
{
    public string Id { get; set; } = string.Empty;
    public string WindowId { get; set; } = string.Empty;
    public string SlotId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public int Capacity { get; set; }
    public int BookedCount { get; set; }
    public int RemainingCapacity { get; set; }
    public IEnumerable<ManageSlotAllocationItemResponse> Allocations { get; set; } = Enumerable.Empty<ManageSlotAllocationItemResponse>();
}
