using System.Text.Json.Serialization;
using VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Helper;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Domain.Store;

public class StoreWorkHours
{
    public string Day { get; set; } = string.Empty;
    public string OpeningTime { get; set; } = string.Empty;
    public string ClosingTime { get; set; } = string.Empty;
    [JsonConverter(typeof(FlexibleBooleanJsonConverter))]
    public bool IsClosed { get; set; }
}
