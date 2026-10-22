using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class OrderInstructions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ItemTypes
        {
            SSR,
            OSI
        }
        public ItemTypes? ItemType { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemText { get; set; }
        public string? Status { get; set; }
        public string? InstructionId { get; set; }
    }
}
