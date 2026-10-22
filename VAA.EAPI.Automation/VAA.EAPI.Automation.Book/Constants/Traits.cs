namespace VAA.EAPI.Automation.Book.Constants
{
    public static class Traits
    {
        public const string BookBffSmoke = "BookBff-Smoke";
        public const string BookBffRegression = "BookBff-Regression";
        public const string BookGraphQLSmoke = "BookGraphQl-Smoke";
        public const string BookGraphQLRegression = "BookGraphQl-Regression";
        public const string BookE2ERegression = "Book-E2E-Regression";
        public const string BookMicroserviceSmoke = "BookMicroservice-Smoke";
        public const string BookMicroserviceRegression = "BookMicroservice-Regression";
        public const string BookSmoke = "Book-Smoke";
        public const string BookCoreRegression = "Book-Core-Regression";
        public const string BookFunctional = "Book-Functional";
        public const string BookAdHoc = "Book-AdHoc";
        public const string NbfGraphQLCICD = "NBFGraphQL_CICD";
        public const string LoyaltyAccountDataCreation = "LoyaltyAccountDataCreation";
        public const string LoyaltyAccountStoragePointsDelete = "LoyaltyAccountStoragePointsDelete";
        public const string LoyaltyAccountStorageDataCopy = "LoyaltyAccountStorageDataCopy";
        public const string LoyaltyAccountStoragePointsUpdate = "LoyaltyAccountStoragePointsUpdate";
        public const string LoyaltyAccountStorageAddPoints = "LoyaltyAccountStorageAddPoints";
        public const string SearchGraphQLRegression = "SearchGraphQl-Regression";
        public const string BrandMicroserviceRegression = "BrandMS-Regression";
        public const string BrandMicroserviceCiCd = "NBFBrandMS_CICD";
        public const string BasketMicroserviceCiCd = "NBFBasketMS_CICD";
        public const string OfferMicroserviceRegression = "OfferMicroservice-Regression";
        public const string SearchBffRegression = "SearchBff-Regression";
        public const string BookFeatureInsurance = "Book-Feature-Insurance";
        public const string BookFeatureMultiCity = "Book-Feature-MultiCity";
        public const string BookFeatureVAC = "Book-Feature-VACAccount";
        public const string BookGraphQLPSSITSanity = "BookGraphQLPSSITSanity";
        public const string BookEdgeCase = "BookEdgeCase";
        public const string BookGlobalProtectRequired = "Book-GlobalProtectRequired";
        public const string SharedServicesSignOff = "SharedServicesSignOff";
        public const string LoyaltyAccountVACAccountCreation = "LoyaltyAccountVACAccountCreation";
        public const string LoyaltyAccountCoBrandDataCreation = "LoyaltyAccountCoBrandDataCreation";
        public const string PNRsWithFlyingClubMembers = "PNRsWithFlyingClubMembers";
        public const string PNRsWithGlassTierDetails = "PNRsWithGlassTierDetails";
        public const string MobileAppPnrsCreationForPartnerAirlines = "MobileAppPnrsCreationForPartnerAirlines";
        public const string MobileAppPnrsSeatsTesting = "MobileAppPnrsSeatsTesting";
        public const string PNRsWithAwardBookings = "PNRsWithAwardBookings";

        // PR-2 of the PNR-provisioner consolidation: new seed flows run under
        // these traits alongside the existing PNRsWith* traits for ~1 week
        // of side-by-side data-quality comparison. Once validated, the old
        // PnrDataCreation.cs and its traits get retired in PR-3.
        public const string PNRsWithFlyingClubMembersViaProvisioner = "PNRsWithFlyingClubMembersViaProvisioner";
        public const string PNRsWithGlassTierDetailsViaProvisioner = "PNRsWithGlassTierDetailsViaProvisioner";
        public const string PNRsWithAwardBookingsViaProvisioner = "PNRsWithAwardBookingsViaProvisioner";
    }
}
