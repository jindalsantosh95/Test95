using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Mapper
{
    public class SkyRetroFeedbackMapper
    {
        public static OnlineFeedbackRequestType SkyFeedbackRequest(FeedbackRequest skyRetroFeedbackRequest)
        {
            return new OnlineFeedbackRequestType
            {
                UniqueIdentificationNumber = skyRetroFeedbackRequest.UniqueIdentificationNumber,
                FFPCarrierCode = skyRetroFeedbackRequest.FFPCarrierCode,
                OperatingCarrierCode = skyRetroFeedbackRequest.OperatingCarrierCode,
                PostingDate = skyRetroFeedbackRequest.PostingDate,
                PostingStatus = skyRetroFeedbackRequest.PostingStatus,
                BaseMiles = skyRetroFeedbackRequest.BaseMiles,
                ClassOfServiceBonusMiles = skyRetroFeedbackRequest.ClassOfServiceBonusMiles,
                EliteTierBonusMiles = skyRetroFeedbackRequest.EliteTierBonusMiles,
                PromotionalBonusMiles = skyRetroFeedbackRequest.PromotionalBonusMiles
            };
        }

        public static SkyOnlineRetroFeedbackResponse SkyOnlineRetroFeedbackResponse(OnlineFeedbackResponseType onlineFeedbackResponseType)
        {
            return new SkyOnlineRetroFeedbackResponse
            {
                TrackID = onlineFeedbackResponseType.TrackID,
                EligibilityDenialCode = onlineFeedbackResponseType.EligibilityDenialCode,
                IsSuccess = onlineFeedbackResponseType.EligibilityDenialCode.Equals(Constants.Application.SkyTeamFeedbackSuccessCode),
                UniqueIdentificationNumber = onlineFeedbackResponseType.UniqueIdentificationNumber
            };
        }
    }
}
