using InfrastructureStoreManagerManageSlotAllocationItemResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageSlotAllocationItemResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

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

    public static explicit operator ManageSlotAllocationItemResponse(InfrastructureStoreManagerManageSlotAllocationItemResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new ManageSlotAllocationItemResponse
        {
            Id = response.Id,
            WindowId = response.WindowId,
            SlotId = response.SlotId,
            StoreId = response.StoreId,
            AllocationIndex = response.AllocationIndex,
            Status = response.Status,
            Reason = response.Reason,
            BookingId = response.BookingId,
            CustomerRef = response.CustomerRef
        };
    }
}
