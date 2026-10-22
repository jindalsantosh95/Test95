using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatColumn
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SeatColumnPositionCodeEnum
        {
            [EnumMember(Value = "W")]
            W = 1,
            [EnumMember(Value = "A")]
            A = 2,
            [EnumMember(Value = "C")]
            C = 3
        }
        public SeatColumnPositionCodeEnum SeatColumnPositionCode { get; set; }
    }
}
