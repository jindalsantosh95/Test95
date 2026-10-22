using System;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignAssign;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignRegister;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignAssign
{
    public class CampaignAssignEventDataMapper
    {
        public static CampaignAssignEventData CampaignAssignRequestToEventDataMapper(CamapignAssignRequestData request, CampaignDetails campaignData)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            return new CampaignAssignEventData
            {
                CustomerId = request.CustomerId,
                MemberNumber = request.MemberNumber,
                CampaignCode = request.CampaignCode,
                CampaignId = request.CampaignId,
                CampaignType = campaignData.CampaignType,
                CreatedBy = request.CreatedBy,
                DocumentStatus = (campaignData.RegistrationRequired == "Y") ?
                 EnumOperations.GetEnumDescription((CampaignDocumentStatus)Enum.Parse(typeof(CampaignDocumentStatus), Convert.ToString(CampaignDocumentStatus.DocumentStatusRegistered)))
                 : EnumOperations.GetEnumDescription((CampaignDocumentStatus)Enum.Parse(typeof(CampaignDocumentStatus), Convert.ToString(CampaignDocumentStatus.DocumentStatusTargeted))),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
            };
        }
    }
}
