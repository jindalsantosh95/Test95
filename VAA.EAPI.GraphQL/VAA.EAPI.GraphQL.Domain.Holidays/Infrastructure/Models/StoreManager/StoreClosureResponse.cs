using Newtonsoft.Json;
using VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Infrastructure.Models.StoreManager;

public class StoreClosureResponse
{
    public string Id { get; set; } = string.Empty;
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly ClosureDate { get; set; }
    public string ClosureType { get; set; } = string.Empty;
    public string? Reason { get; set; }
}
