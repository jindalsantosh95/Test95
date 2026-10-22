using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums
{
    public enum CustomerEventType
    {
        [EnumMember(Value = "VAA.PowerApp.Customer.AssignLoyaltyAward")]
        AssignLoyaltyAward,

        [EnumMember(Value = "VAA.Loyalty.Audit.CustomerProfileEvent")]
        CreateCustomerProfile
    }
}
