using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class DimensionDetail
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum DimensionTypeCodeEnum
        {
            [EnumMember(Value = "LENGTH")]
            LENGTH = 1,
            [EnumMember(Value = "WIDTH")]
            WIDTH = 2,
            [EnumMember(Value = "HEIGHT")]
            HEIGHT = 3,
            [EnumMember(Value = "LINEAR")]
            LINEAR = 4
        }
        public DimensionTypeCodeEnum DimensionTypeCode { get; set; }
        public Dimension Dimension { get; set; }
    }
}
