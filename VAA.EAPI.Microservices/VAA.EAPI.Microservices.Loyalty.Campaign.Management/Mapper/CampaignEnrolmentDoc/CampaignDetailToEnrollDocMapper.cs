using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignEnrolmentDoc;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignEnrolmentDoc
{
    public static class CampaignDetailToEnrollDocMapper
    {
        public static CampaignEnrolmentDocDetails MaptoEnrollmentDoc(CampaignRegisterEventData campaignData, string promotionId)
        {
            if (campaignData is null) throw new ArgumentNullException(nameof(campaignData));

            return new CampaignEnrolmentDocDetails
            {
                CampaignEnrolmentDocument = new()
                {
                    CustomerId = campaignData.CustomerId,
                    PromotionId = promotionId,
                    MemberNumber = campaignData.MemberNumber,
                    CreatedBy = campaignData.CreatedBy
                }
            };
        }
    }
}
