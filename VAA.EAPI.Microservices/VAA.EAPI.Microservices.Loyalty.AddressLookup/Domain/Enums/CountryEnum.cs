using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Enums
{
    public enum Country
    {
        [EnumMember(Value = "GBR")]
        GB = 0,
        [EnumMember(Value = "USA")]
        US = 1,
    }
}
