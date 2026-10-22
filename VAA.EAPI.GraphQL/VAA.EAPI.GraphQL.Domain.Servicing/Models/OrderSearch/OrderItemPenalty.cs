using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class OrderItemPenalty
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum Types
        {
            Change,
            Cancellation
        }
        public Types? Type { get; set; }
        public Amount? Amount { get; set; }
        public int Percentage { get; set; }
        public string? Link { get; set; }
        public List<string>? Text { get; set; }
    }
}