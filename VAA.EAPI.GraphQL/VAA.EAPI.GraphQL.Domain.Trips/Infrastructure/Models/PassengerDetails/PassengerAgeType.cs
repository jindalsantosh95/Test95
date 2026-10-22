using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PassengerAgeType
    {
        ADULT = 1,
        YOUNG_ADULT = 2,
        CHILD = 3,
        INFANT_WITH_SEAT = 4,
        INFANT_IN_ARMS = 5
    }
}