using InfrastructureStoreManagerManageSlotAllocationBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.ManageSlotAllocationBookingResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class ManageSlotAllocationBookingResponse
{
    public string BookingId { get; set; } = string.Empty;
    public string CustomerRef { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public static explicit operator ManageSlotAllocationBookingResponse(InfrastructureStoreManagerManageSlotAllocationBookingResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new ManageSlotAllocationBookingResponse
        {
            BookingId = response.BookingId,
            CustomerRef = response.CustomerRef,
            Status = response.Status,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt
        };
    }
}
