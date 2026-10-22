using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Application.Command.StoreAppointment;

public class AppointmentBookingRequest
{
    [JsonConverter(typeof(StringEnumConverter))]
    public AppointmentType? AppointmentType { get; set; }
    public string? AppointmentReason { get; set; }
    public string? BookingReference { get; set; }
    public DateOnly? Date { get; set; }
    public DateTimeOffset? SlotStart { get; set; }
    public DateTimeOffset? SlotEnd { get; set; }
    public string? LocationId { get; set; }
    public string? VisitPurpose { get; set; }
    public string? VisitPurposeDetails { get; set; }
    public string? AdditionalDetails { get; set; }
    public string? SearchParameter { get; set; }
    public BookingCustomer? Customer { get; set; }
}
