namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class BookingNotificationRequest
{
    public string RecipientToken { get; set; } = string.Empty;
    public string? Template { get; set; }
    public string? Locale { get; set; }
}
