using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Enums
{
    public enum LoyaltyMemberEventType
    {
        [EnumMember(Value = "BasicEmailEvent")]
        BirthdayEmailEvent,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateFlyingClubMember")]
        UpdateFlyingClubMemberEvent,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateGroupDocument")]
        UpdateGroupDocEvent,
        [EnumMember(Value = "VAA.PowerApp.Customer.CreateGroupDocument")]
        CreateGroupDocEvent,
        [EnumMember(Value = "BasicEmailEvent")]
        BasicEmailEvent,
        [EnumMember(Value = "VAA.Loyalty.MemberMgmt.DelinkHouseholdAccounts")]
        DeleteAccountOnMemberUpdateEvent,
        [EnumMember(Value = "VAA.PowerApp.RuleEngine.CreateTransaction")]
        TransactionCreateActionExecution,
        [EnumMember(Value = "VAA.Vred.CreateTransaction")]
        VredCreateTransactionEvent,
        [EnumMember(Value = "VAA.Loyalty.MemberMgmt.AddressChange")]
        AddressChangeEvent,
        [EnumMember(Value = "VAA.Loyalty.Customer.UpdateCustomerSearchDocument")]
        UpdateCustomerSearchDocEvent,
        [EnumMember(Value = "VAA.Loyalty.Customer.UpdateCustomerDocument")]
        UpdateCustomerDocEvent,
        [EnumMember(Value = "VAA.Loyalty.Customer.CreateAddressHistoryDocument")]
        CreateAddressHistoryDocEvent,
        [EnumMember(Value = "VAA.Order.ManageSpecialService")]
        AddSSREvent,
        [EnumMember(Value = "VAA.Loyalty.Audit.OtherEvent")]
        SupplementarySilverAuditCreateEvent,
        [EnumMember(Value = "VAA.Loyalty.MemberMgmt.FCStampingResponseFile")]
        FCStampingResponseFileEvent,
    }
}
