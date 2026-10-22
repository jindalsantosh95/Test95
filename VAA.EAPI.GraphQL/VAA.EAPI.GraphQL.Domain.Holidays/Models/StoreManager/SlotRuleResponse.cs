using InfrastructureStoreManagerSlotRuleResponse = VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager.SlotRuleResponse;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Models.StoreManager;

public class SlotRuleResponse
{
    public string Id { get; set; } = string.Empty;
    public int DayOfWeek { get; set; }
    public string StartTime { get; set; } = string.Empty;
    public string EndTime { get; set; } = string.Empty;
    public int SlotDurationMinutes { get; set; }
    public int SlotCapacity { get; set; }

    public static explicit operator SlotRuleResponse(InfrastructureStoreManagerSlotRuleResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        return new SlotRuleResponse
        {
            Id = response.Id,
            DayOfWeek = response.DayOfWeek,
            StartTime = response.StartTime,
            EndTime = response.EndTime,
            SlotDurationMinutes = response.SlotDurationMinutes,
            SlotCapacity = response.SlotCapacity
        };
    }
}
