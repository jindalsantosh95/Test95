using System.Runtime.Serialization;


namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Enums
{
    public enum RuleEngineEventType
    {
        [EnumMember(Value = "VAA.PowerApp.RuleEngine.ActionExecution")]
        RuleEngineActionExecution,
        [EnumMember(Value = "VAA.PowerApp.RuleEngine.UpdateFlyingClubMember")]
        FcmUpdateActionExecution,
        [EnumMember(Value = "VAA.PowerApp.RuleEngine.UpdateMembershipChange")]
        MembershipChangeUpdateActionExecution,
        [EnumMember(Value = "VAA.PowerApp.RuleEngine.CreateMembershipChange")]
        MembershipChangeCreateActionExecution,
        [EnumMember(Value = "VAA.PowerApp.RuleEngine.CreateTransaction")]
        TransactionCreateActionExecution
    }
}
