using InfrastructureStoreManagerManageSlotAllocationResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageSlotAllocationResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

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

    public static explicit operator ManageSlotAllocationResponse(InfrastructureStoreManagerManageSlotAllocationResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new ManageSlotAllocationResponse
        {
            Id = response.Id,
            WindowId = response.WindowId,
            SlotId = response.SlotId,
            StoreId = response.StoreId,
            StartAt = response.StartAt,
            EndAt = response.EndAt,
            Capacity = response.Capacity,
            BookedCount = response.BookedCount,
            RemainingCapacity = response.RemainingCapacity,
            Allocations = response.Allocations?.Select(x => (ManageSlotAllocationItemResponse)x) ??
                          Enumerable.Empty<ManageSlotAllocationItemResponse>()
        };
    }
}
