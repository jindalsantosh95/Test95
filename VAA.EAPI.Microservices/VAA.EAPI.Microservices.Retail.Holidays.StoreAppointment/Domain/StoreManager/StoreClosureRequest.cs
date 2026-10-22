namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class StoreClosureRequest
{
    public DateOnly ClosureDate { get; set; }
    public string ClosureType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

