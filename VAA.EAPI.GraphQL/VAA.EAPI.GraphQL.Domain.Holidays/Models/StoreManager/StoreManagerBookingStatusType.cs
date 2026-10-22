using HotChocolate.Types;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public sealed class StoreManagerBookingStatusType : EnumType<StoreManagerBookingStatus>
{
    protected override void Configure(IEnumTypeDescriptor<StoreManagerBookingStatus> descriptor)
    {
        descriptor.Name("StoreManagerBookingStatus");
        descriptor.Value(StoreManagerBookingStatus.Booked).Name("BOOKED");
        descriptor.Value(StoreManagerBookingStatus.Cancelled).Name("CANCELLED");
    }
}
