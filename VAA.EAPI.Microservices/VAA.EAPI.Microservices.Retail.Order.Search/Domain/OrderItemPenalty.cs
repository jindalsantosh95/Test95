using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class OrderItemPenalty
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Types
        {
            [EnumMember(Value = "Change")]
            BagEnum = 0,
            [EnumMember(Value = "Cancellation")]
            SeatEnum = 1
        }
        public Types? Type { get; set; }
        public Amount Amount { get; set; }
        public int Percentage { get; set; }
        public string Link { get; set; }
        public List<string> Text { get; set; }
    }
}