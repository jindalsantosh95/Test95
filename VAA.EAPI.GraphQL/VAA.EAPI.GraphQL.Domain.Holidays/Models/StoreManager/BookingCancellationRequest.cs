namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public sealed class BookingCancellationRequest
{
    public BookingCancellationReason Reason { get; set; }
    public bool CustomerContactConfirmed { get; set; }
}
