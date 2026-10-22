using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignRegister
{
    public class CampaignRegisterEventDataMapper
    {
        public static CampaignRegisterEventData CampaignRegisterRequestToEventDataMapper(CamapignRegisterRequestData request, CampaignDetails campaignDetails)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            return new CampaignRegisterEventData
            {
                MemberNumber = request.MemberNumber,
                CustomerId = request.CustomerId,
                CampaignCode = request.CampaignCode,
                CampaignId = request.CampaignId,
                Email = request.Email,
                CreatedBy = request.CreatedBy,
                FirstName = request.FirstName,
                LastName = request.LastName,
                DocumentId = request.DocumentId,
                DocumentStatus = EnumOperations.GetEnumDescription((CampaignDocumentStatus)Enum.Parse(typeof(CampaignDocumentStatus), CampaignDocumentStatus.DocumentStatusRegistered.ToString())),
                CampaignType = campaignDetails.CampaignType,
                CampaignEndDate = campaignDetails.CampaignEndDate

            };
        }
    }
}
