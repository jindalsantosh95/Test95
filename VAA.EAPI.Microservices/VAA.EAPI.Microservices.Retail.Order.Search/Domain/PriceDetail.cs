using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Domain
{
    public class PriceDetail
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PriceComponentTypes
        {
            [EnumMember(Value = "BasePrice")]
            BasePriceEnum = 0,
            [EnumMember(Value = "BaseFare")]
            BaseFareEnum = 1,
            [EnumMember(Value = "Tax")]
            TaxEnum = 2,
            [EnumMember(Value = "Fee")]
            FeeEnum = 3,
            [EnumMember(Value = "Surcharge")]
            SurchargeEnum = 4,
            [EnumMember(Value = "TotalTax")]
            TotalTaxEnum = 5,
        }
        public PriceComponentTypes? PriceComponentType { get; set; }
        public string PriceComponentName { get; set; }
        public Amount Amount { get; set; }
        public BaseCurrencyAmount BaseCurrencyAmount { get; set; }
    }
}