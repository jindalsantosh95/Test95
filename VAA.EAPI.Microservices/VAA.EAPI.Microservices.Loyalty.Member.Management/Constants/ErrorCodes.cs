namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Constants
{
    public class ErrorCodes
    {
        public static readonly string MiscellaneousTechnicalError = "G009";
        public static readonly string MandatoryInputFieldsMissing = "15001";
        public static readonly string InvalidDateFormat = "10003";
        public static readonly string InvalidCustomerId = "10002";
        public static readonly string SameHoHAccountAndChildAccount = "10004";
        public static readonly string InvalidHeadOfAccountEmailId = "10005";
        public static readonly string InvalidChildAccountEmailId = "10006";
        public static readonly string FCMemberAccountNotActive = "10007";
        public static readonly string HoHLinkingPermittedOnlyForGoldAndSilver = "10008";
        public static readonly string HoHEmailTriggerError = "10009";
        public static readonly string HoHDelinkGroupIdNotFoundError = "10010";
        public static readonly string AccountIsNotHoHForAddressUpdate = "10011";
        public static readonly string SSAInvalidAccountStatus = "10012";
        public static readonly string SupplimentarySilverAccountNotFound = "10013";
        public static readonly string SSAInvalidChildTierCode = "10014";
        public static readonly string SSAInvalidParentTierCode = "10015";
        public static readonly string SSAlinkedGoldParentAccount = "10016";
        public static readonly string SSASameParentAndChild = "10017";
        public static readonly string SSAInvalidChildStatus = "10018";
        public static readonly string SSAInvalidParentStatus = "10019";
        public static readonly string InvalidFcNumber = "10164";
        public static readonly string InvalidMemberStatus = "10168";
        public static readonly string InvalidAddressMatch = "10166";
        public static readonly string ExistingMemberInvite = "10167";
    }
}

