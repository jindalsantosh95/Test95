using Newtonsoft.Json;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.StoreManager;

public class SlotAllocationItemResponse
{
    public string Id { get; set; } = string.Empty;
    public string WindowId { get; set; } = string.Empty;
    public string SlotId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public int AllocationIndex { get; set; }
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    public SlotStatus Status { get; set; } = SlotStatus.Available;
    public string? Reason { get; set; }
    public string? BookingId { get; set; }
    public string? CustomerRef { get; set; }
}
