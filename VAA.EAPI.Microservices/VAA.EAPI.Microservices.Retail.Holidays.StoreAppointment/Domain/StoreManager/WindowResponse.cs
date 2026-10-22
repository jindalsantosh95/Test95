using Newtonsoft.Json;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class WindowResponse
{
    public string Id { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }
    public int Capacity { get; set; }
    public int RemainingCapacity { get; set; }
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public SlotStatus Status { get; set; } = SlotStatus.Available;
    public string? Reason { get; set; }
}
