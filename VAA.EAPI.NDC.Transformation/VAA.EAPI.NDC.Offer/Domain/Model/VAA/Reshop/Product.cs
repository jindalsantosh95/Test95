using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.Reshop
{
    public class Products
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ProductTypeEnum
        {
            [EnumMember(Value = "Bag")]
            Bag = 0,
            [EnumMember(Value = "Seat")]
            Seat = 1,
            [EnumMember(Value = "Flight")]
            Flight = 2
        }

        public ProductTypeEnum? ProductType { get; set; }
        public string ProductOwner { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}
