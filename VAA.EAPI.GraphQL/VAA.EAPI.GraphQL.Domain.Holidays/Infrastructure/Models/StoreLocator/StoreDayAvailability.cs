namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreDayAvailability
{
    public DateTime Date { get; set; }
    public bool HasAvailableSlot { get; set; }
}