namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreSlotsRecord
{
    public string StoreId { get; set; } = string.Empty;
    public IEnumerable<StoreSlot> Slots { get; set; } = Enumerable.Empty<StoreSlot>();
}
