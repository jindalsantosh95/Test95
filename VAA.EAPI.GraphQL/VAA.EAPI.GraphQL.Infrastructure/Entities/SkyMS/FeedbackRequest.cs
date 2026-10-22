using System;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS
{
    public class FeedbackRequest
    {
        public string UniqueIdentificationNumber { get; set; }
        public string FFPCarrierCode { get; set; }
        public string OperatingCarrierCode { get; set; }
        public string PostingDate { get; set; }
        public string PostingStatus { get; set; }
        public string BaseMiles { get; set; }
        public string ClassOfServiceBonusMiles { get; set; }
        public string EliteTierBonusMiles { get; set; }
        public string PromotionalBonusMiles { get; set; }

        public static explicit operator FeedbackRequest((CheckEligibilityClaimResponse skyResponse, SegmentResponse inRuleResponse) input)
        {
            var skyFlightResponse = input.skyResponse.SkyClaimResponse;
            var inRuleResponse = input.inRuleResponse;
            return new FeedbackRequest()
            {
                UniqueIdentificationNumber = skyFlightResponse.UniqueIdentificationNumber,
                FFPCarrierCode = skyFlightResponse.FFPCarrierCode,
                OperatingCarrierCode = skyFlightResponse.OperatingCarrierCode,
                PostingStatus = Common.Constants.Application.PostingStatusFlag,
                BaseMiles = Convert.ToString(inRuleResponse.BasePoints),
                ClassOfServiceBonusMiles = Convert.ToString(inRuleResponse.CabinBonusPoints),
                EliteTierBonusMiles = Convert.ToString(inRuleResponse.TierBonusPoints)
            };
        }
    }
}
