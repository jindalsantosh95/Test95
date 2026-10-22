using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class OrderInstructions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ItemTypes
        {
            [EnumMember(Value = "SSR")]
            SSREnum = 0,
            [EnumMember(Value = "OSI")]
            OSIEnum = 1
        }
        public ItemTypes? ItemType { get; set; }
        public string ItemCode { get; set; }
        public string ItemText { get; set; }
        public string Status { get; set; }
        public string InstructionId { get; set; }
    }
}
