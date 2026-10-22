using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.OrderSearch
{
    public class PriceDetail
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PriceComponentTypes
        {
            BasePrice,
            BaseFare,
            Tax,
            Fee,
            Surcharge,
            TotalTax
        }
        public PriceComponentTypes? PriceComponentType { get; set; }
        public string? PriceComponentName { get; set; }
        public Amount? Amount { get; set; }
    }
}