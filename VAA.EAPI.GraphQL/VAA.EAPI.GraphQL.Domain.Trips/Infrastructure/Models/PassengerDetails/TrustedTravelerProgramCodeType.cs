using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TrustedTravelerProgramCodeType
    {
        KNOWN_TRAVELER_NUMBER = 1,
        REDRESS_NUMBER = 2,
        CANADA_TRAVELER_NUMBER = 3
    }
}