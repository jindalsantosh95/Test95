namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment.Models;

public class StoreCalendarAvailabilityResultRow
{
    public string StoreId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public DateOnly Date { get; set; }
    public bool HasAvailableSlot { get; set; }
}
