namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.StoreAppointment.Models;

public class StoreSlotResult
{
    public string StoreId { get; set; } = string.Empty;
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public bool IsAvailable { get; set; }
}