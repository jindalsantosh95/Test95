using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums
{
    public enum VredStatusEnum
    {
        [EnumMember(Value = "Active")]
        Active = 0,
        [EnumMember(Value = "Expired")]
        Expired = 1,
        [EnumMember(Value = "Merged")]
        Merged = 2,
        [EnumMember(Value = "Suspended")]
        Suspended = 3,
        [EnumMember(Value = "Active Pending")]
        ActivePending = 4,
        [EnumMember(Value = "Fraud")]
        Fraud = 5,
        [EnumMember(Value = "Inactive")]
        Inactive = 6,

    }
}
