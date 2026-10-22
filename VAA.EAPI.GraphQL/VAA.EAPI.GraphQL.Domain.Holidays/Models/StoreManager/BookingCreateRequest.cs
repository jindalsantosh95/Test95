namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class BookingCreateRequest
{
    public string StoreId { get; set; } = string.Empty;
    public string WindowId { get; set; } = string.Empty;
    public string CustomerRef { get; set; } = string.Empty;
    public BookingNotificationRequest? Notification { get; set; }
}
