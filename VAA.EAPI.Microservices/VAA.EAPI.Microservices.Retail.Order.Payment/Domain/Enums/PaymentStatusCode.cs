using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaymentStatusCode
    {
        [EnumMember(Value = "Authorised")]
        Authorised = 2000,
        [EnumMember(Value = "Declined")]
        Declined = 2011,
    }
}
