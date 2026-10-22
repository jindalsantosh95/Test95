namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

internal sealed class StoreManagerBookingConflictRow
{
    public DateOnly RangeStart { get; set; }
    public DateOnly RangeEndExclusive { get; set; }
    public DateOnly Date { get; set; }
    public int ActiveBookingCount { get; set; }
    public string Reason { get; set; } = string.Empty;
}
