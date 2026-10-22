namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class StoreManagerSlotAllocationStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
