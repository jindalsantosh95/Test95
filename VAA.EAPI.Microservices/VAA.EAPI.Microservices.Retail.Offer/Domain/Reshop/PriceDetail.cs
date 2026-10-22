using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop
{
    public class PriceDetail
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PriceComponentTypes
        {
            [EnumMember(Value = "BasePrice")]
            BasePrice = 0,
            [EnumMember(Value = "BaseFare")]
            BaseFare = 1,
            [EnumMember(Value = "Tax")]
            Tax = 2,
            [EnumMember(Value = "Fee")]
            Fee = 3,
            [EnumMember(Value = "Surcharge")]
            Surcharge = 4,
            [EnumMember(Value = "TotalTax")]
            TotalTax = 5,
            [EnumMember(Value = "EquivalentFare")]
            EquivalentFareEnum = 7
        }
        public PriceComponentTypes? PriceComponentType { get; set; }
        public string PriceComponentName { get; set; }
        public CurrencyAmount Amount { get; set; }
        public BaseCurrencyAmount BaseCurrencyAmount { get; set; }
        public IEnumerable<Tax> TaxBreakUp { get; set; }
    }

}