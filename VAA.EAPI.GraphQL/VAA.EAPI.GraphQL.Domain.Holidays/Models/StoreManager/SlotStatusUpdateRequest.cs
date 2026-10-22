namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class SlotStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
}
