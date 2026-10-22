using InfrastructureStoreManagerBookingResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.BookingResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

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

    public static explicit operator BookingResponse(InfrastructureStoreManagerBookingResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new BookingResponse
        {
            Id = response.Id,
            StoreId = response.StoreId,
            WindowId = response.WindowId,
            CustomerRef = response.CustomerRef,
            Status = response.Status,
            CreatedAt = response.CreatedAt,
            UpdatedAt = response.UpdatedAt,
            NotificationStatus = response.NotificationStatus,
            NotificationMessage = response.NotificationMessage
        };
    }
}
