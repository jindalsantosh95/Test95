using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;

public class AppointmentBookingResponse
{
    public string AppointmentReferenceId { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Store { get; set; } = string.Empty;
    public StoreSlot SlotDetails { get; set; } = new();
}
