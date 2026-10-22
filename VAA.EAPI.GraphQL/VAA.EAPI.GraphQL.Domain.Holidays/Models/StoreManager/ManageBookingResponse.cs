using InfrastructureStoreManagerManageBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageBookingResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

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

    public static explicit operator ManageBookingResponse(InfrastructureStoreManagerManageBookingResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new ManageBookingResponse
        {
            Id = response.Id,
            StoreId = response.StoreId,
            StoreSlotId = response.StoreSlotId,
            CustomerRef = response.CustomerRef,
            Status = response.Status,
            WindowStartAt = response.WindowStartAt,
            WindowEndAt = response.WindowEndAt,
            WindowCapacity = response.WindowCapacity,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };
    }
}
