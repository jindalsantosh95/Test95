namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class SlotAllocationResponse
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
    public IReadOnlyCollection<SlotAllocationItemResponse> Allocations { get; set; } = Array.Empty<SlotAllocationItemResponse>();
}
