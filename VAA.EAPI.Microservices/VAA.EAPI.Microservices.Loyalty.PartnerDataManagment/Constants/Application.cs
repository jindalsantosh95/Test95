namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Constants
{
    public class Application
    {
        public const string UpdateFlownDetailsForEligibleRetro = "[STG].[UpdateFlownDetailsForValidRetro]";
        public const string UpdateFeedbackDetails = "[STG].[UpdateFeedbackDetails]";
        public const string GetPassengerDetails = "[STG].[GetPassengerDetails]";
        public const string ResetPendingFeedBackAutoRetro = "[STG].[ResetPendingFeedBackAutoRetro]";
        public const string ResetPendingFeedBackAutoRetroProcessSuccessMessage = "ResetPendingFeedBackAutoRetroProcessSuccessMessage";
        public const string GetVSFlownDetails = "[STG].[GetFlownTransactionData]";
        public const string UpdateVSRetroFlownDetails = "[STG].[UpdateFlownDetails]";
        public const string UpdateFlownSegmentStatus = "[STG].[UpdateFlownTransactionData]";
        public const string SuccessStatusUpdate = "Ticket Number Updated successfully";
        public const string FailureStatusUpdate = "No status updated for given Ticket Number";
        public const string ClaimStatus = "FREE";

        //MVT Subscriber
        public const string PdmEventTopicName = "pdmeventmessages";
        public const string MVTMessageSubscriptionName = "MVTSubscriber";
        public const string FlightMessageEventType = "FlightArrivalMessage";
        public const string EventTimeFormat = "yyyy-MM-dd'T'HH:mm:ss'Z'";
        public const string EventSpecversion = "1.0";
        public const string FlightMessageTopicName = "FlightArrivalDetailsTopic";
        public const string MVTMessageEventType = "MVTMessages";
        public const string MVTEventStoreTable = "MVTMessagesTable";

        ///Flown Passenger Subscriber
        public const string InsertFlownPassengersDetail = "dbo.SP_INSERT_FLOWNDATA_FILE_DTL_NLA_TEST";
        public const string FlownPassengerDataSubscriber = "FlownPassengerDataSubscriber";
        public const string FlownPassengerTopicName = "pdmeventmessages";
        public const string AutoAccrualMessage = "SkyTeamAutoAccrualMessages";

        //Auto Accrual Subscriber
        public const string SkyTeamAutoAccrualSubscriptionName = "SkyTeamAutoAccrualSubscriber";
        public const string ReferenceTableName = "ReferenceDataTable";
        public const string PartnerPartitionKey = "Partner";
        public const string SkyTeamOutboundFeedbackDataTable = "SkyTeamOutboundFeedbackDataTable";
        public const string SkyTeamOutboundFeedbackMessageLookupTable = "SkyTeamOutboundFeedbackMessageLookupTable";
        public const string AutoAccrualFeedbackOutboundMessagesPartitionKey = "AutoAccrualFeedbackOutboundMessages";
        public const string AutoAccrualFeedbackOutboundType = "AutoAccrualFeedbackOutboundMessage";
        public const string GetFlownPassengersForAutoAccrual = "[STG].[GetFlownPassengersForAutoAccrual]";
        public static readonly List<string> ValidProcessingStatuses = ["Free"];
        public const string NoShowNo = "N";
        public const string SentToPartnerNo = "N";

        public const string SkyTeamInboundFeedbackProcessingSubscriptionName = "SkyTeamInboundFeedbackProcessingSubscriber";
        public const string SkyTeamInboundFeedbackGenerationSubscriptionName = "SkyTeamInboundFeedbackGenerationSubscriber";
        public const string AutoAccrualFeedbackProcessingStatus = "Processing";
        public const string AutoAccrualFeedbackProcessedSuccessfullyStatus = "Processed Successfully";
        public const string AutoAccrualFeedbackErrorStatus = "Error";
        public const string AutoAccrualFeedbackSentStatus = "Feedback Sent";
        public const string UpdateAutoAccrualProcessingStatus = "[STG].[UpdateAutoAccrualProcessingStatus]";
        public const string AllocatedProcessingStatus = "Allocated";
        public const string FreeProcessingStatus = "Free";
        public const string FeedbackDbaSchemaId = "nla.feedback.dba";
        public const int FeedbackDbaSchemaVersion = 1;
        public const int NlaPartyCodeMaxLength = 3;
        public const int NlaEncryptionKeyIdMaxLength = 50;
        public const int NlaEncryptedPayloadMaxLength = 6144;
        public const string NlaCanonicalGuidFormat = "D";
        public const string NlaAirlineCodePattern = "^[A-Z]{2,3}$";
        public const string NlaProductCodePattern = "^[A-Z]{3}$";
        public const int UniqueIdentificationNumberMaxLength = 21;
        public const string SuccessfulPostingStatus = "Y";
        public const string UnsuccessfulPostingStatus = "N";
        public const int NlaAdditionalInformationMaxLength = 25;
        public const int NlaMinimumErrorDetailsCount = 1;
        public const string NlaErrorCodePattern = "^\\d{3}\\.\\d{3}$";
        public const int NlaErrorDescriptionMaxLength = 50;
        public const int NlaSchemaVersion = 1;
        public const string AutoAccrualRequestType = "AA01";
        public const string DistanceBasedAccrualType = "DBA";
        public const string SkyTeamInboundConsumer = "VS";

        //NLA Mapper
        public const string AccrualDbaSchemaId = "nla.accrual.dba";
        public const int AccrualDbaSchemaVersion = 1;
        public const string AccrualDbaRequestType = "AA01";
        public const string AccrualDbaAccrualType = "DBA";
        public const string AccrualDbaProductCode = "FLT";
        public const string AccrualDbaAdditionalInformation = "NDC";
        public const string GetVSRetroFlownDetails = "[STG].[GetFlownData]";
        public const string GetFlownPassengersForAutoAccrualFallback = "[STG].[GetFlownPassengersForAutoAccrualFallback]";
        public const string SkyTeamPartnersCacheKey = "SkyTeamPartners";

        //SkyTeam Inbound Auto Accrual
        public const string SkyTeamInboundAutoAccrualProcessingSubscriptionName = "SkyTeamInboundAutoAccrualProcessingSubscriber";
        public const string ErrorStatus = "ERROR";

        // VS retro claim input validation
        public const string FareBasisCodesBlobName = "fare-basis-codes.json";
        public const string PassengerTypeCodesBlobName = "passenger-type-codes.json";
        public const string TicketDesignatorCodesBlobName = "ticket-designator-codes.json";
        public const string BookingNotEligible = "Booking is not eligible for accrual (discounted fare)";
        public const string VSClaimInputsRequired = "At least one of TicketDesignator, FareBasis or PassengerType is required.";
        public const string FlownSuccessStatusUpdate = "VS Flown Details Updated successfully";
        public const string FlownFailureStatusUpdate = "VS Flown Details Update Failed";
    }
}
