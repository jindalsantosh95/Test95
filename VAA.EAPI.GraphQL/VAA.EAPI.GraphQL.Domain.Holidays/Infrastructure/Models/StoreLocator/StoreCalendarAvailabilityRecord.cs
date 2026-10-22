namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreCalendarAvailabilityRecord
{
    public string StoreId { get; set; } = string.Empty;
    public int Year { get; set; }
    public int Month { get; set; }
    public IEnumerable<StoreDayAvailability> Days { get; set; } = Enumerable.Empty<StoreDayAvailability>();
}
