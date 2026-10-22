using VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.AppointmentBooking;

public class AppointmentBookingResponse
{
    public string AppointmentReferenceId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Store { get; set; } = string.Empty;
    public StoreSlot SlotDetails { get; set; } = new();
}
