using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.Enums
{
    public enum DocumentTypes
    {
        [EnumMember(Value = "T")]
        TEnum = 0,
        [EnumMember(Value = "Y")]
        YEnum = 1,
        [EnumMember(Value = "INF")]
        INFEnum = 2
    }
}
