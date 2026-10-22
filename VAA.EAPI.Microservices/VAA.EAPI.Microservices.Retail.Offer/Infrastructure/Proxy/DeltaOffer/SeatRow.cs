using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class SeatRow
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum SeatStatusCodeEnum
        {
            [EnumMember(Value = "A")]
            A = 1,
            [EnumMember(Value = "T")]
            T = 2,
            [EnumMember(Value = "U")]
            U = 3,
            [EnumMember(Value = "N")]
            N = 4,
            [EnumMember(Value = "R")]
            R = 5
        }
        public SeatStatusCodeEnum SeatStatusCode { get; set; }
        public string RowNum { get; set; }
        public IEnumerable<string> CharacteristicCode { get; set; }
    }
}
