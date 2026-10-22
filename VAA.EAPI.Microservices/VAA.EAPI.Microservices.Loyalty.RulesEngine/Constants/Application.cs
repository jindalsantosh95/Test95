namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Constants
{
    public class Application
    {
        // Enviromental settings
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static string CommonSQLRefDBMIToken;

        // Service Bus
        public static readonly string SpecVersion = "1.0";

        // Command Response 
        public static readonly string HeaderParameterlocation = "location";

        //InRule
        public const string RuleEngineEventTopicName = "ruleengineeventmessages";
        public const string TierRuleSetEventSubscriptionName = "TierRuleSetMessageSubscriber";
        public const string CombinedRuleSetEventSubscriptionName = "CombinedRuleSetMessageSubscriber";
        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public const string ActionExecutionSubscriptionName = "ActionExecutionMessageSubscriber";

        public const string SystemName = "LOOPS";
        public const string ActiveFlagValue = "Y";
        public const string InActiveFlagValue = "N";
        public const string CardStatus = "active";
        public const string GoldTier = "Gold";
        public const string TierApprovalStatus = "Automated";
        public const string StraightToGoldFlag = "S2GFlag";
        public const string StraightToGoldFlagAction = "Set S2G Flag";
        public const string StraightToSilverFlag = "S2SFlag";
        public const string StraightToSilverFlagAction = "Set S2S Flag";
        public const string StraightToSilverFTGFlag = "S2SFTGFlag";
        public const string StraightToSilverFTGFlagAction = "Set S2S FTG Flag";
        public const string BirthdayBonusProductName = "Birthday Bonus";
        public const string AssignPointsAction = "Assign Points";
        public const string SetEarnActivityFlagAction = "Set Earn Activity";
        public const string EarnActivityFlag = "EarnActivity";
        public const string AccrualTrxnType = "Accrual";
        public const string AccrualTrxnSubType = "Product";
        public const string MilesPointType = "Miles";
        public const string BaseMilesPointSubType = "BaseMiles";
        public const string BirthdayBonusUser = "Birthday Bonus";
        public const string BirthdayBonusTrxnChannel = "Auto Created";
        public const string SuccessTrxnStatus = "Processed";
        public const string VSPartnerCode = "VS";
        public const string BirthdayStatement = "A gift from Flying Club for your birthday";
        public const string MilesQualifyingFlag = "N";
        public static readonly string VSPartnerIdCacheKey = "ReferenceData_PARTNERID_CACHE_KEY";
        public static readonly string TxnDateTimeFormat = "yyyy-MM-dd hh:mm:ss";
        public static readonly string LoyaltyProgramRule = "LoyaltyProgramRules";
        public static readonly string BasicPointAwardRule = "BasicPointAward";
    }
}