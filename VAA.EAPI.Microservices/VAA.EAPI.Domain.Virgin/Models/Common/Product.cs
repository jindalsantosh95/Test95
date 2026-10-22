using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.Domain.Virgin.Models.Common
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
            FlightEnum = 2,
            [EnumMember(Value = "NameCorrection")]
            NameCorrection = 3
        }
        public ProductTypes? ProductType { get; set; }
    }
}
