namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;

public class StoreAppointmentEmailEventData
{
    public string AppointmentReferenceId { get; set; } = string.Empty;
    public string TemplateId { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public string EmailDataXml { get; set; } = string.Empty;
}
