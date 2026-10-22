using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PassengerDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum NotificationActionType
    {
        DISMISS = 1
    }
}