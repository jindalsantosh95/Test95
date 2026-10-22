using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;

public class Appointment
{
    public string Id { get; set; } = string.Empty;
    public AppointmentType AppointmentType { get; set; } = AppointmentType.InStore;
    public string AppointmentReason { get; set; } = string.Empty;
    public string BookingReference { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public TimeOnly SlotStart { get; set; }
    public TimeOnly SlotEnd { get; set; }
    public string LocationId { get; set; } = string.Empty;
    public string VisitPurpose { get; set; } = string.Empty;
    public string VisitPurposeDetails { get; set; } = string.Empty;
    public string AdditionalDetails { get; set; } = string.Empty;
    public string? AllocationId { get; set; }
    public string AppointmentStatus { get; set; } = Constants.Constants.BookedAppointmentStatus;
    public DateTimeOffset CreatedUtc { get; set; } = DateTimeOffset.UtcNow;
}
