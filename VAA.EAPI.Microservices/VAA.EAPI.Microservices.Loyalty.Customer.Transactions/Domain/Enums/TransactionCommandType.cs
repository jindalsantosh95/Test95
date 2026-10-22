
using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Enums
{
    public enum TransactionCommandType
    {
        [EnumMember(Value = "NotSet")]
        NotSet,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreateContactNote")]
        CreateContactNote,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateContactNote")]
        UpdateContactNote,
        [EnumMember(Value = "VAA.PowerApp.Customer.DeleteContactNote")]
        DeleteContactNote,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreateAward")]
        CreateAward,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateAward")]
        UpdateAward,
        [EnumMember(Value = "VAA.PowerApp.Customer.DeleteAward")]
        DeleteAward,

        [EnumMember(Value = "VAA.Loyalty.Audit.AwardAudit")]
        AwardAudit,

        [EnumMember(Value = "VAA.PowerApp.Customer.AssignTopMember")]
        AssignTopMember,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreateTargetedMembership")]
        CreateTargetedMembership,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateTargetedMembership")]
        UpdateTargetedMembership,
        [EnumMember(Value = "VAA.PowerApp.Customer.DeleteTargetedMembership")]
        DeleteTargetedMembership,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreateCampaignEnrolmentDoc")]
        CreateCampaignEnrolment,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreateMembershipChange")]
        CreateMembershipChange,
        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateMembershipChange")]
        UpdateMembershipChange,

        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateFlyingClubMember")]
        UpdateFlyingClubMemberEvent,

        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateFCMForEditCenturionCard")]
        UpdateFCMForEditCenturionCardEvent,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreateTransaction")]
        CreateTransaction,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreatePartnerMembership")]
        CreatePartnerMembership,

        [EnumMember(Value = "VAA.VAA.Loyalty.Audit.CardLinkingAudit")]
        CardLinkingAudit,

        [EnumMember(Value = "VAA.PowerApp.Customer.UpdatePartnerMembership")]
        UpdatePartnerMembership,

        [EnumMember(Value = "VAA.PowerApp.Customer.EnrichAuditDtWithCustDtls")]
        CustomerProfileAudit,

        [EnumMember(Value = "VAA.PowerApp.Customer.CancelTransaction")]
        CancelTransaction,

        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateTransaction")]
        UpdateTransaction,

        [EnumMember(Value = "VAA.MyAccount.Customer.UpdateBanner")]
        UpdateBanner,

        [EnumMember(Value = "VAA.PowerApp.Customer.CreateMemberCustomAttributes")]
        CreateMemberCustomAttributes,

        [EnumMember(Value = "VAA.PowerApp.Customer.UpdateMemberCustomAttributes")]
        UpdateMemberCustomAttributes,

        [EnumMember(Value = "VAA.Loyalty.Customer.RedeemVoucher")]
        RedeemVoucher,

        [EnumMember(Value = "VAA.Vred.CreateTransaction")]
        VredCreateTransaction,

        [EnumMember(Value = "VAA.Loyalty.Customer.RollbackVoucherRedemption")]
        RollbackVoucherRedemption,

        [EnumMember(Value = "VAA.Vred.CancelTransaction")]
        VredCancelTransaction,

        [EnumMember(Value = "VAA.Loyalty.Customer.UpdateVoucherRelatedTxnsPnr")]
        UpdateVoucherRelatedTxnsPnr,

        [EnumMember(Value = "VAA.Vred.UpdateTransactionPnr")]
        VredUpdateTransactionPnr,

        [EnumMember(Value = "VAA.Loyalty.Voucher.TransactionMonitoringStatus")]
        VoucherMonitoringStatus,
    }
}
