using InfrastructureStoreSlot = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator.StoreSlot;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreLocator;

public class StoreSlot
{
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public bool IsAvailable { get; set; }

    public static explicit operator StoreSlot(InfrastructureStoreSlot slotDetails)
    {
        ArgumentNullException.ThrowIfNull(slotDetails);

        return new StoreSlot
        {
            StartTime = slotDetails.StartTime,
            EndTime = slotDetails.EndTime,
            IsAvailable = slotDetails.IsAvailable
        };
    }
}
