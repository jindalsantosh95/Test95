namespace VAA.EAPI.Automation.MyAccount.Constants
{
    public static class Traits
    {
        public const string MyAccountGQLDevDTScripts = "GQL-DevAndDT";
        public const string MyAccountGQLRegressionTests = "MyAccountGQLRegressionTests";
        public const string MemberAccountDetailsGraphQl = "MemberAccountDetailsGraphQl";
        public const string GqlEntraAuthentication = "GqlEntraAuthentication";
        // Selects MyAccount GraphQL tests whose operation or resolver flow invokes or depends on IdentityMS.
        public const string IdentityMSRelatedGraphqltests = "IdentityMSRelatedGraphqltests";
        // Selects MyAccount GraphQL tests by the verified downstream microservices used by their resolver paths.
        public const string MemberManagementMSRelatedGraphqltests = "MemberManagementMSRelatedGraphqltests";
        public const string VRedMSRelatedGraphqltests = "VRedMSRelatedGraphqltests";
        public const string CustomerMSRelatedGraphqltests = "CustomerMSRelatedGraphqltests";
        public const string CustomerTransactionMSRelatedGraphqltests = "CustomerTransactionMSRelatedGraphqltests";
        public const string LoyaltyAccountDataCreation = "LoyaltyAccountDataCreation";
        public const string LoyaltyHouseHoldAccountDataCreation = "LoyaltyHouseHoldAccountDataCreation";
        public const string LoyaltyHouseHoldAccountDataUpdate = "LoyaltyHouseHoldAccountDataUpdate";
        public const string LoyaltyHighFiveDataCreation = "LoyaltyHighFiveDataCreation";
        public const string LoyaltyAccountDataCreationToCsv = "LoyaltyAccountDataCreationToCsv";
        public const string LoyaltyVoucherDataCreation = "LoyaltyVoucherDataCreation";
        public const string LoyaltyVoucherDataUpdate = "LoyaltyVoucherDataUpdate";
        public const string LoyaltyAccountStoragePointsDelete = "LoyaltyAccountStoragePointsDelete";
        public const string LoyaltyAccountStorageDataDelete = "LoyaltyAccountStorageDataDelete";
        public const string LoyaltyVoucherStorageDataDelete = "LoyaltyVoucherStorageDataDelete";
        public const string LoyaltyAccountStorageDataCopy = "LoyaltyAccountStorageDataCopy";
        public const string LoyaltyAccountStoragePointsUpdate = "LoyaltyAccountStoragePointsUpdate";
        public const string LoyaltyAccountStorageAddPoints = "LoyaltyAccountStorageAddPoints";
        public const string LoyaltyAccountVACAccountCreation = "LoyaltyAccountVACAccountCreation";
        public const string LoyaltyAccountCoBrandDataCreation = "LoyaltyAccountCoBrandDataCreation";
        public const string AmexPlatinumCardCampaignTests = "AmexPlatinumCardCampaignTests";
        public const string LocalRun = "LocalRun";
        public const string AccountOTPGenerate = "AccountOTPGenerate";
        public const string AccountOTPValidate = "AccountOTPValidate";
        public const string UpdatePreference = "UpdatePreference";
        public const string MemberVoucherDetails = "MemberVoucherDetails";
        public const string SharedServicesSignOff = "SharedServicesSignOff";
    }
}
