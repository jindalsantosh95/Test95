using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class Price
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
            [EnumMember(Value = "TotalPoints")]
            PointsEnum = 6,
        }
        public PriceComponentTypes? PriceComponentType { get; set; }
        public string PriceComponentName { get; set; }
        public CurrencyAmount Amount { get; set; }
        public IEnumerable<Tax> TaxBreakup { get; set; }
        public BaseCurrencyAmount BaseCurrencyAmount { get; set; }
        public PointsAmount? PointsAmount { get; set; }
    }
}