using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class HealthStatus
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusTextEnum
        {
            [EnumMember(Value = "pass")]
            Pass = 1,
            [EnumMember(Value = "fail")]
            Fail = 2,
            [EnumMember(Value = "warn")]
            Warn = 3
        }
        public StatusTextEnum StatusText { get; set; }
    }
}
