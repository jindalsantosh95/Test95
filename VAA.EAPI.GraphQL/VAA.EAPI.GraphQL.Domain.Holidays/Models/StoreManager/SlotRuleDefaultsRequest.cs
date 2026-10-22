namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class SlotRuleDefaultsRequest
{
    public int SlotDurationMinutes { get; set; }
    public int SlotCapacity { get; set; }
    public bool OverwriteExisting { get; set; }
}

