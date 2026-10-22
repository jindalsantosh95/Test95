namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

internal sealed class StoreManagerBookingRow
{
    public string? BookingId { get; set; }
    public string? StoreId { get; set; }
    public int StoreSlotId { get; set; }
    public string? CustomerRef { get; set; }
    public string? Status { get; set; }
    public DateTimeOffset CreatedUtc { get; set; }
    public DateTimeOffset UpdatedUtc { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public int Capacity { get; set; }
}