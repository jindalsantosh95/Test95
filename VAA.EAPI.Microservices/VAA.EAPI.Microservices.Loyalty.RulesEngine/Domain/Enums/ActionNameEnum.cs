using System.Runtime.Serialization;


namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Enums
{
    public enum ActionNameEnum
    {
        [EnumMember(Value = "SET TIER")]
        SetTier,
        [EnumMember(Value = "SET S2G FLAG")]
        SetS2GFlag,
        [EnumMember(Value = "SET S2S FLAG")]
        SetS2SFlag,
        [EnumMember(Value = "SET S2S FTG FLAG")]
        SetS2SFTGFlag,
        [EnumMember(Value = "SET EARN ACTIVITY")]
        SetEarnActivityFlag,
        [EnumMember(Value = "ASSIGN POINTS")]
        AssignPoints,
        [EnumMember(Value = "SET DOWNGRADE PROHIBIT FLAG")]
        SetDowngradeProhibitFlag
    }
}
