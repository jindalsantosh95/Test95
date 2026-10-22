namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class StoreClosureResponse
{
    public string Id { get; set; } = string.Empty;
    public DateOnly ClosureDate { get; set; }
    public string ClosureType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

