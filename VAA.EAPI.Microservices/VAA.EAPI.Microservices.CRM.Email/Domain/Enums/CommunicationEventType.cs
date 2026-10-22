using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.CRM.Email.Domain.Enums
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum CommunicationEventType
    {
        [EnumMember(Value = "VAA.Order.Payment.InitiateCPDErrorCommunication")]
        CPDErrorCommunicationInitiate,
    }
}
