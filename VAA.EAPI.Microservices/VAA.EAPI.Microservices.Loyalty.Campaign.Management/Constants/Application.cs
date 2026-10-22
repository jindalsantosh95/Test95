namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Constants
{
    public class Application
    {
        public static string CommonSQLRefDBMIToken;
        public const string CampaignRegisterEventSubscriptionName = "campaignmanagementregistereventsubscriber";
        public static readonly string CloudEventTimeFormat = "yyyy-MM-dd hh:mm:ss tt";
        public const string CampaignRegisterEventSubscriptionNameEmail = "emailsubscriber";
        public const string CampaignManagementEventTopicName = "campaignmanagementeventmessages";
        public const string EmailEventSubscriptionName = "emailsubscriber";
        public const string EnrolDocStatus = "Registered";
        public const string TargetedMembershipDocUpdateSubscriptionName = "targetedmembershipdocupdateubscriber";
        public const string TargetedMembershipDocSubscriptionName = "targetmembershipeventsubscriber";
        public const string CampaignEnrolmentDocEventSubscriberName = "campaignenrollmentdocsubscriber";
        public static readonly string CampaignEnrolmentDocEventSubject = "VAA.PowerApp.Customer.CampaignEnrolmentDocEvent";
        #region CampaignUpdate
        public const string CampaignUpdateEventHandlerTopicName = "campaignmanagementeventmessages";
        public const string CampaignUpdateEventSubscriptionName = "campaignmanagementupdateventsubscriber";
        public static readonly string HeaaderParameterlocation = "location";
        public static readonly string ISODateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'";
        public static readonly string EventSpecversion = "1.0";
        public static readonly string GetPromotionQuery = "EXEC[dbo].[SP_GET_PROMOTION] @CampaignID";
        public static readonly string CampaignUpdateQuery = "EXEC[dbo].[SP_CAMPAIGN_UPSERT] @CampaignId, @CampaignName, @StartDate, @EndDate, @CampaignCode, @Status, @Type, @Description, @RegistrationRequiredFlag, @LinkToTemplate, @InitiatedBy, @Result OUTPUT";
        public static readonly int UpdateResultNumber = 11;
        public static readonly string EmailSentMessage = "Email Sent successfully";
        public static readonly string CampaignSuccessMessage = "Campaign details updated successfully";
        public const string CampaignCodeAlreadyUsed = "AlreadyUsedCampaignCode";
        public const string InvalidCampaignCode = "InvalidCampaignCode";
        public static readonly string UpdateTargetedMembershipComment = "Updated Targeted Membership as part of campaign registration";
        public static readonly string Faliure = "Faliure";
        public static readonly string Success = "Success";
        public static readonly string TargetedDocUpdateSuccessMessage = "TargetedMembership document updated successfully";
        // Service Bus
        public static readonly string SpecVersion = "1.0";
        // Command Response 
        public static readonly string HeaderParameterlocation = "location";
        #endregion
        public const string TargetedMembershipDocCreateSubscriptionName = "targetedmembershipdoccreatesubscriber";
        public static readonly string AssignSuccessMessage = "Campaign assign initiated successfully";
        public static readonly string TargetedDocSuccessMessage = "TargetedMembership event posted successfully";
        public const string CreateTargetedMembershipComment = "New Targeted Membership";
        public static readonly string CampaignAssignInitiated = "Campaign assign initiated successfully";
        public static readonly string CampaignRegistrationInitiated = "Campaign registration initiated successfully";
        public const string CampaignRegisterEventSubscriptionNameProfile = "memeberprofileeventsubscriber";
        public static readonly string MemberProfileUpdateMessage = "MemberProfileUpdate event posted successfully";
        public const string CampaignAssignEventSubscriptionName = "campaignassigneventsubscriber";
        #region MemberProfileUpdate
        public const string FastTrackToGold = "Fast Track to Gold";
        public const string GoldAfterFirstFlight = "Gold After First Flight";
        public const string SilverAfterFirstFlightWithFTG = "Silver After 1st Flight + FTG";
        public const string SilverAfterFirstFlight = "Silver After First Flight";
        public const string StraightToGold = "Straight to Gold";
        public const string StraightToSilver = "Straight to Silver";
        public const string StraightToSilverWithFTG = "Straight to Silver with FTG";
        public static readonly string StraightToSilverFlag = "S2SFlag";
        public static readonly string StraightToGoldFlag = "S2GFlag";
        public static readonly string StraightToSilverFTGFlag = "S2SFTGFlag";
        public static readonly string ProfileFlag = "Y";
        public static readonly string EmailTemplate = "FCGEN-21";
        #endregion
        #region CampaignCreate
        public const string CampaignCreateEventSubscriptionName = "campaignmanagementcreateeventsubscriber";
        public static readonly string CampaignCreateQuery = "EXEC[dbo].[SP_CAMPAIGN_CREATE] @CampaignName, @StartDate, @EndDate, @CampaignCode, @Status, @Type, @Description, @RegistrationRequiredFlag, @LinkToTemplate, @InitiatedBy, @Result OUTPUT";
        public static readonly int CreateResultNumber = 10;
        public static readonly string CreateFailed = "Failure";
        public static readonly string CreatedSuccessfully = "Success";
        public static readonly string DuplicateCampaign = "AlreadyUsedCampaignCode";
        public const string InvalidCampaignType = "InvalidCampaignType";
        public static readonly string CampaignCreateSuccessMessage = "Campaign created successfully";
        #endregion
        public const string CampaignPromotionLinkEventSubscriptionName = "campaignpromotionlinkeventsubscriber";
        public static readonly string CampaignPromotionLinkQuery = "EXEC[dbo].[SP_CAMPAIGNPROMOTION_MAP_CREATE] @CampaignId, @PromotionId, @CreatedBy, @LastUpdatedBy, @Result OUTPUT";
        public const string LinkingAlreadyExist = "AlreadyLinkedCampaignAndPromotion";
        #region CampaignTypeRetrieval
        public const string CampaignType = "CAMPAIGN_TYPE";
        public const string CampaignSubType = "Standalone Campaign";
        public const string ActiveList = "Y";
        #endregion
        #region PromotionUpdate        
        public const string PromotionUpdateEventSubscriptionName = "promotionmanagementupdateventsubscriber";
        public static readonly string PromotionUpdateQuery = "EXEC[dbo].[SP_PROMOTION_UPDATE] @PromotionId, @PromotionName, @StartDate, @EndDate, @Active,@PartnerName,@InitiatedBy,@Result OUTPUT";
        public static readonly int PromotionUpdateResultNumber = 7;
        public static readonly string PromotionUpdateSuccessMessage = "Promotion details updated successfully";
        public const string PromotionNameAlreadyUsed = "PromotionNameAlreadyUsed";
        public const string InvalidPromotionId = "InvalidPromotionId";
        #endregion
        #region CampaignUn-assign        
        public static readonly string campaignUnassignSuccessMessage = "Un-assign campaign completed successfully";
        public const string campaignUnassignFailureMessage = "Un-assign campaign has errors";
        #endregion
        #region PromotionSearch
        public static readonly string PromotionSearchQuery = "EXEC[dbo].[SP_PROMOTION_SEARCH] @PromotionId, @PromotionName, @StartDateF, @EndDateF, @StartDateT,@EndDateT,@PartnerName, @Active";
        #endregion
        public const string CampaignPromotionDelinkEventSubscriptionName = "campaignpromotiondelinkeventsubscriber";
        public static readonly string PromotionLinkSuccessMessage = "Campaign Promotion linked successfully";
        public static readonly string PromotionDelinkSuccessMessage = "Campaign Promotion delinked successfully";
        public static readonly string CampaignPromotionDelinkQuery = "EXEC[dbo].[SP_CAMPAIGNPROMOTION_MAP_DELETE] @CampaignId, @PromotionId, @Result OUTPUT";
        public const string LinkingNotExist = "NoLinkFoundForCampaignAndPromotion";
        #region PromotionCreate        
        public const string PromotionCreateEventSubscriptionName = "promotionmanagementcreateventsubscriber";
        public static readonly string PromotionCreateSuccessMessage = "Promotion created successfully";
        public static readonly string PromotionCreateQuery = "EXEC[dbo].[SP_PROMOTION_CREATE] @PromotionName, @StartDate, @EndDate, @Active,@PartnerName, @InitiatedBy,@Result OUTPUT";
        public static readonly int PromotionCreateResultNumber = 6;
        public const string InvalidPartnerName = "InvalidPartnerName";
        public static readonly string PromotionCampaignSearchQuery = "EXEC[dbo].[SP_PROMOTION_CAMPAIGN_SEARCH] @PromotionId";
        #endregion
        #region PromotionsListDownload  
        //public static readonly string PromotionsList = "select Name, Description from Promotions";
        public static readonly string PromotionsList = "EXEC[dbo].[SP_GET_PROMOTIONS_EXTRACT]";
        public static readonly string PromotionsWorksheetName = "Promotions";
        public static readonly string PromoTableFirstColumn = "PromotionId";
        public static readonly string PromoTableSecondColumn = "PromotionName";
        public static readonly string PromoTableThirdColumn = "PartnerName";
        public static readonly string PromoTableFourthColumn = "Active";
        public static readonly string PromoTableFifthColumn = "StartDate";
        public static readonly string PromoTableSixthColumn = "EndDate";
        public static readonly string PromoTableLastColumn = "Campaign Linked";
        public static readonly string PromotionsBlobName = "PromotionsList";
        #endregion
        public const string PdmChannel = "PDM";
        public static readonly string CampaignDetailsCacheKey = "ReferenceData_CAMPAIGNDETAILS_CACHE_KEY";
        #region CampaignSearch
        public static readonly string CampaignSearchQuery = "EXEC[dbo].[SP_CAMPAIGN_SEARCH] @Name, @StartDateF, @EndDateF, @StartDateT, @EndDateT, @CampaignCode, @Status, @Type, @CampaignId";
        public static readonly string GetAssociatedMemCountQuery = "EXEC [dbo].[SP_CAMPAIGN_COUNT] @CampaignCode,@MemberCount OUTPUT";
        #endregion
        #region CampaignsDownload
        public static readonly string CampaignsWorksheetName = "Campaigns";
        public static readonly string[] CampaignstWorksheetColumns = { "Campaign Code", "Campaign Name", "Status", "Type", "Start Date", "End Date", "Campaign Description", "Link to Template", "Registration required Flag" };
        public static readonly string CampaignsBlobName = "Campaigns";
        public static readonly string GetAssociatedMembers = "EXEC [dbo].[SP_CAMPAIGN_MEMBERS] @CampaignCode";
        public static readonly string AssocMemsWorksheetName = "AssociatedMembers";
        public static readonly string[] AssocMemsWorksheetColumns = { "Members", "Campaign Code" };
        #endregion
        #region UniqueIdValidation
        public const string CampaignAmexPartitionKey = "PCM";
        public const string CampaignUniqueIdPartitionKey = "PCM";
        public const string CampaignUniqueIdTableName = "CampaignUniqueIdTable";
        #endregion

    }
}
