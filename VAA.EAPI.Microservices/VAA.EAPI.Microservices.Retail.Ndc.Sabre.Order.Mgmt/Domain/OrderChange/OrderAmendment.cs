using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange
{
    public class OrderAmendment
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProductTypes
        {
            [EnumMember(Value = "Bag")]
            BagEnum = 0,
            [EnumMember(Value = "Seat")]
            SeatEnum = 1,
            [EnumMember(Value = "Flight")]
            FlightEnum = 2,
            [EnumMember(Value = "SSR")]
            SSREnum = 3
        }
        [OpenApiProperty(Description = "Indicates the type (category) of product ")]
        public ProductTypes? ProductType { get; set; }
        [OpenApiProperty(Description = "The customer (a unqiuely identifiable customer or a PTC - passenger type code) price for the product offering.PTC: adult, young adult, child, infant etc. ")]
        public IEnumerable<OrderByReference> OrderByReference { get; set; }
        public CustomerPrice CustomerPrices { get; set; }
        public IEnumerable<FlightItems> FlightItems { get; set; }
        public IEnumerable<string> CustomerReference { get; set; }
        public IEnumerable<SpecialServiceRequestInfo> SpecialServiceRequestInfo { get; set; }
    }
}
