using System.Runtime.Serialization;

namespace VAA.EAPI.GraphQL.Domain.Account.Constants
{
    public enum AccountBannerCode
    {
        VAC_TO_FC_UPGRADE
    }

    public enum VoucherStatus
    {
        Available,
        Gifted,
        Used,
        Expired,
        Cancelled,
        Active
    }

    public enum VoucherCategory
    {
        REWARD,
        CLUBHOUSE,
        DISCOUNT
    }

    public enum VoucherSource
    {
        [EnumMember(Value = "VIRGIN RED US CREDIT CARD")]
        VIRGIN_RED_US_CREDIT_CARD,
        [EnumMember(Value = "VIRGIN CREDIT CARD UK")]
        VIRGIN_CREDIT_CARD_UK,
        [EnumMember(Value = "VIRGIN CREDIT CARD US")]
        VIRGIN_CREDIT_CARD_US,
        [EnumMember(Value = "VIRGIN ATLANTIC AIRWAYS")]
        VIRGIN_ATLANTIC_AIRWAYS,
        [EnumMember(Value = "VIRGIN MONEY")]
        VIRGIN_MONEY
    }

    public enum VoucherTypes
    {
        [EnumMember(Value = "FLYING CLUB REWARD VOUCHER - VIRGIN RED US CREDIT CARD")]
        FLYING_CLUB_REWARD_VOUCHER_VIRGIN_RED_US_CREDIT_CARD,

        [EnumMember(Value = "GOLD LOUNGE PASS")]
        GOLD_LOUNGE_PASS,

        [EnumMember(Value = "FLYING CLUB REWARD VOUCHER - UK REWARD CREDIT CARD")]
        FLYING_CLUB_REWARD_VOUCHER_UK_REWARD_CREDIT_CARD,

        [EnumMember(Value = "FLYING CLUB REWARD VOUCHER - UK REWARD+ CREDIT CARD")]
        FLYING_CLUB_REWARD_VOUCHER_UK_REWARD_PLUS_CREDIT_CARD,

        [EnumMember(Value = "FLYING CLUB REWARD VOUCHER - GOLD 2000 TIER POINTS")]
        FLYING_CLUB_REWARD_VOUCHER_GOLD_2000_TIER_POINTS,

        [EnumMember(Value = "FLYING CLUB REWARD VOUCHER - GOLD RENEWAL")]
        FLYING_CLUB_REWARD_VOUCHER_GOLD_RENEWAL
    }
}
