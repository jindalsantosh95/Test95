using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.MessageDetails
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageContextCodeType
    {        
        PNR,        
        TRIP,        
        SEGMENT
    }
}
