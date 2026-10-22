using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class ProductGroup
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProductGroupNameEnum
        {
            [EnumMember(Value = "Seats")]
            Seats = 1,
            [EnumMember(Value = "Upsell")]
            Upsell = 2,
            [EnumMember(Value = "Bags")]
            Bags = 3,
            [EnumMember(Value = "Allowance")]
            Allowance = 4,
            [EnumMember(Value = "Embargoes")]
            Embargoes = 5,
            [EnumMember(Value = "Disclosure")]
            Disclosure = 6,
            [EnumMember(Value = "AddOn")]
            AddOn = 7,
            [EnumMember(Value = "Wifi")]
            Wifi = 8
        }
        public ProductGroupNameEnum ProductGroupName { get; set; }
        public IEnumerable<string> ProductIds { get; set; }
    }
}
