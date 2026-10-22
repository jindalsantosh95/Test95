namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Appointment;

public class StoreAppointmentSaveResult
{
    public bool IsSuccess { get; set; }
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
}
