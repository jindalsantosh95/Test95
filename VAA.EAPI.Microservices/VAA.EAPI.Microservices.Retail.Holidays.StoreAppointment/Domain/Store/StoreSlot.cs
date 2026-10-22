namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreSlot
{
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public bool IsAvailable { get; set; }
}
