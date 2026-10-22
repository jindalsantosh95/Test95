namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class BookingNotificationRequest
{
    public string RecipientToken { get; set; } = string.Empty;
    public string? Template { get; set; }
    public string? Locale { get; set; }
}

