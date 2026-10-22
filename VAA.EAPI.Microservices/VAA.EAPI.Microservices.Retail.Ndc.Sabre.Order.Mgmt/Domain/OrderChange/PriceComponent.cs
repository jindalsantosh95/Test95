using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using VAA.EAPI.Domain.Virgin.Models.Common;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class PriceComponent
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
            SurchargeEnum = 4
        }
        [OpenApiProperty(Description = "Component type. for example Tax,Basefare etc.")]
        public PriceComponentTypes? PriceComponentType { get; set; }
        [OpenApiProperty(Description = "The name give to the instance of the price component type e.g. YQ")]
        public string PriceComponentName { get; set; }
        public CurrencyAmount Amount { get; set; }
    }
}
