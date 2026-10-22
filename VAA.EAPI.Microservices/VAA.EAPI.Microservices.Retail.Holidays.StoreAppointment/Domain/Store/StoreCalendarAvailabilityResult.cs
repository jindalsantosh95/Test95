namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreCalendarAvailabilityResult
{
    public string StoreId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public IEnumerable<StoreDayAvailability> Days { get; set; } = Enumerable.Empty<StoreDayAvailability>();
}