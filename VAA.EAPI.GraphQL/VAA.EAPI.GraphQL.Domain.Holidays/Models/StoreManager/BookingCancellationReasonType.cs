using HotChocolate.Types;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public sealed class BookingCancellationReasonType : EnumType<BookingCancellationReason>
{
    protected override void Configure(IEnumTypeDescriptor<BookingCancellationReason> descriptor)
    {
        descriptor.Name("BookingCancellationReason");
        descriptor.Value(BookingCancellationReason.StoreClosure).Name("STORE_CLOSURE");
    }
}
