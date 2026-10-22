using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Holidays.StoreAppointment.Constants
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AppointmentType
    {
        InStore,
        CallBack
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum SlotStatus
    {
        [EnumMember(Value = "AVAILABLE")]
        Available,

        [EnumMember(Value = "BOOKED")]
        Booked,

        [EnumMember(Value = "BLOCKED")]
        Blocked
    }
}
