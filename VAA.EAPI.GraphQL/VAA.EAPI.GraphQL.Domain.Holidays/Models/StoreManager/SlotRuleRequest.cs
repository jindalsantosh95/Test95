namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class SlotRuleRequest
{
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public int SlotCapacity { get; set; }
}

