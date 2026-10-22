using System.Runtime.Serialization;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums
{
    public enum CorporateCampaignTypes
    {
        [EnumMember(Value = "Accrual")]
        Accrual,
        [EnumMember(Value = "Corp - Gold 2 Sectors JCDI")]
        CorpGoldToSectorsJCDI,
        [EnumMember(Value = "Corporate Offer - Bonus Miles")]
        CorporateOfferBonusMiles,
        [EnumMember(Value = "Gold After First Flight")]
        GoldAfterFirstFlight,
        [EnumMember(Value = "Silver After 1st Flight + FTG")]
        SilverAfterFirstFlightFTG,
        [EnumMember(Value = "Silver After First Flight")]
        SilverAfterFirstFlight,
        [EnumMember(Value = "Straight to Gold")]
        StraightToGold,
        [EnumMember(Value = "Straight to Silver")]
        StraightToSilver,
        [EnumMember(Value = "Straight to Silver with FTG")]
        StraightToSilverWithFTG
    }
}
