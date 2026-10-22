using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class TimeLimit
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Types
        {
            [EnumMember(Value = "TicketingTimeLimit")]
            TicketingTimeLimit = 0,
            [EnumMember(Value = "PaymentTimeLimit")]
            PaymentTimeLimit = 1
        }
        public Types? Type { get; set; }
        public string Limit { get; set; }
        public string Remarks { get; set; }
    }
}
