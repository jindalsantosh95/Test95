using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AdvancePassengerInformationCodeType
    {
        BASIC_INFO = 1,
        TRUSTED_TRAVELER_PROGRAM = 2,
        PASSENGER_CONTACT = 3,
        EMERGENCY_CONTACT = 4,
        DESTINATION_ADDRESS = 5
    }
}