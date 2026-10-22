using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums
{
    public enum CustomerTypeEnum
    {

        Basic,
        Loyalty,
        VirginAccount
    }

    public enum Country
    {
        [EnumMember(Value = "GBR")]
        GB = 0,
        [EnumMember(Value = "USA")]
        US = 1,
    }
}
