using System.Text.Json.Serialization;
using VAA.EAPI.GraphQL.Domain.Holidays.Helper;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreLocator;

public class StoreWorkHours
{
    public string Day { get; set; } = string.Empty;
    public string OpeningTime { get; set; } = string.Empty;
    public string ClosingTime { get; set; } = string.Empty;
    [JsonConverter(typeof(FlexibleBooleanJsonConverter))]
    public bool IsClosed { get; set; }
}
