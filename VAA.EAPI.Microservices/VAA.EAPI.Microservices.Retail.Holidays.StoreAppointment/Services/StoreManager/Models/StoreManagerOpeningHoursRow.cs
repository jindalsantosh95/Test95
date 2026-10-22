namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreManager.Models;

internal sealed class StoreManagerOpeningHoursRow
{
    public string? OpeningHoursId { get; set; }
    public string? StoreId { get; set; }
    public byte DayOfWeek { get; set; }
    public TimeSpan OpensAt { get; set; }
    public TimeSpan ClosesAt { get; set; }
    public bool IsClosed { get; set; }
}