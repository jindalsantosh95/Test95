using InfraStoreSlotsRecord = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreSlotsRecord;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

public class StoreSlotsResponse
{
    public string StoreId { get; set; } = string.Empty;
    public IEnumerable<StoreSlot> Slots { get; set; } = Enumerable.Empty<StoreSlot>();

    public static explicit operator StoreSlotsResponse(InfraStoreSlotsRecord infraRecord)
    {
        ArgumentNullException.ThrowIfNull(infraRecord);

        return new StoreSlotsResponse
        {
            StoreId = infraRecord.StoreId,
            Slots = infraRecord.Slots.Select(slot => new StoreSlot
            {
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                IsAvailable = slot.IsAvailable
            })
        };
    }
}
