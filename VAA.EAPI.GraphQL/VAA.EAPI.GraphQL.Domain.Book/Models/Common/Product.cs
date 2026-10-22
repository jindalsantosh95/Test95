using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.Common
{
    public class Product
    {
        public string ProductOwner { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProductTypes
        {
            [EnumMember(Value = "Bag")]
            BagEnum = 0,
            [EnumMember(Value = "Seat")]
            SeatEnum = 1,
            [EnumMember(Value = "Flight")]
            FlightEnum = 2
        }
        public ProductTypes? ProductType { get; set; }
    }
}
