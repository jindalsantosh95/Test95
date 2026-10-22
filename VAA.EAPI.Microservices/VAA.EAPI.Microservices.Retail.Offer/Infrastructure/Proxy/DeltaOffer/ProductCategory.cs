using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class ProductCategory
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProductCategoryCodeEnum
        {
            [EnumMember(Value = "FLIGHTS")]
            FLIGHTS = 1,
            [EnumMember(Value = "SEATS")]
            SEATS = 2,
            [EnumMember(Value = "UPSELL")]
            UPSELL = 3,
            [EnumMember(Value = "BAGS")]
            BAGS = 4,
            [EnumMember(Value = "ADDON")]
            ADDON = 5,
            [EnumMember(Value = "WIFI")]
            WIFI = 6
        }
        public ProductCategoryCodeEnum ProductCategoryCode { get; set; }
        public string ProductCategoryText { get; set; }
        public IEnumerable<string> ProductIds { get; set; }
    }
}
