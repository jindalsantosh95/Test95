namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager;

public sealed class BookingCancellationRequest
{
    public string Reason { get; set; } = string.Empty;
    public bool CustomerContactConfirmed { get; set; }
    public string CancelledByUserId { get; set; } = string.Empty;
    public string? CancelledByDisplayName { get; set; }
}
