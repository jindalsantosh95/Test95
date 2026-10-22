using System.Runtime.Serialization;
namespace VAA.EAPI.Microservices.Retail.Sustainability.Domain
{
    public enum CabinType
    {
        [EnumMember(Value = "Economy")]
        Economy,
        [EnumMember(Value = "Business")]
        Business,
        [EnumMember(Value = "Premium")]
        Premium,
        [EnumMember(Value = "First")]
        First
    }
}
