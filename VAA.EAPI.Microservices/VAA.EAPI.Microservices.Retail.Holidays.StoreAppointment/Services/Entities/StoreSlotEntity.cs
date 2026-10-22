namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Services.Entities;

public class StoreSlotEntity
{
    public int StoreSlotId { get; set; }
    public string StoreId { get; set; } = string.Empty;
    public string DayCode { get; set; } = string.Empty;
    public TimeOnly SlotStartTime { get; set; }
    public TimeOnly SlotEndTime { get; set; }
    public int Capacity { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}
