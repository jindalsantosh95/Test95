namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Constants
{
    public class ErrorCodes
    {
        public static readonly string MiscellaneousTechnicalError = "G009";
        public static readonly string MandatoryInputFieldsMissing = "15001";
        public static readonly string InvalidCampaignId = "10002";
        public static readonly string CampaignCodeAlreadyUsed = "10004";
        public static readonly string InvalidDateFormat = "10003";
        public static readonly string InvalidCampaignType = "10005";
        public static readonly string PromotionNameAlreadyUsed = "10006";
        public static readonly string InvalidPromotionId = "10007";
        public static readonly string LinkingAlreadyExist = "10008";
        public static readonly string InvalidPartnerName = "10009";
        public static readonly string LinkingNotExist = "10010";
        public static readonly string CampaignNotEligibleForEmail = "10011";
        public static readonly string CampaignCodeAlreadyAssigned = "10012";
        public static readonly string PcmUniqueIdInvalid = "10014";
    }
}
