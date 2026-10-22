using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RequestOptionCodeType
    {
        CONFIRMATION = 1,
        TICKET = 2
    }
}