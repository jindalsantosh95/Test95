namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreSlot
{
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
