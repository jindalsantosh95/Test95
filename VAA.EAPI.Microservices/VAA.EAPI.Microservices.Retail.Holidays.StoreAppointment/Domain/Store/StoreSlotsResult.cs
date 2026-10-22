namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreSlotsResult
{
    public string StoreId { get; set; } = string.Empty;
    public IEnumerable<StoreSlot> Slots { get; set; } = Enumerable.Empty<StoreSlot>();
}
