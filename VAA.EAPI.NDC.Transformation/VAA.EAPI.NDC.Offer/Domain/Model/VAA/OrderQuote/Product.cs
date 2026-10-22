using System.Runtime.Serialization;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA.OrderQuote
{
    public class Products
    {
        public enum ProductTypeEnum
        {
            [EnumMember(Value = "Bag")]
            Bag = 1,
            [EnumMember(Value = "Seat")]
            Seat = 2,
            [EnumMember(Value = "Flight")]
            Flight = 3
        }

        public ProductTypeEnum? ProductType { get; set; }
        public string ProductOwner { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
    }
}
