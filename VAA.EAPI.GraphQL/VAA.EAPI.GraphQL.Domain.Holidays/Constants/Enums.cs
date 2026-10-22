using System.Text.Json.Serialization;
using HotChocolate;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Holidays.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AppointmentType
    {
        InStore = 0,
        CallBack = 1
    }

    public enum BookingCancellationReason
    {
        [GraphQLName("STORE_CLOSURE")]
        StoreClosure
    }

    public enum StoreManagerBookingStatus
    {
        [GraphQLName("BOOKED")]
        Booked,

        [GraphQLName("CANCELLED")]
        Cancelled
    }
}

