using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;
using CloudEvent = VAA.EAPI.Automation.MarketingLoyalty.Requests.Events.CloudEvent;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper
{
    public class CampaignRegisterEventMapper
    {

        public static CloudEvent MapCampaignRegisterEvent(CampaignRegister campaignRegisterDetails, string serviceBusTopicName)
        {
            return new CloudEvent
            {
                Id = Guid.NewGuid().ToString(),
                Data = campaignRegisterDetails,
                Type = Constants.Application.CampaignRegsiterEvent,
                Source = serviceBusTopicName,
                Subject = Constants.Application.CampaignRegsiterEvent,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Specversion = Constants.Application.SpecVersion,
                DataContentType = Constants.Application.DataContentType
            };
        }

        public static CampaignRegister CampaignRegisterDetails(string customerId, string campaignCode, string campaignId, string createdBy, string email, string firstName, string lastName, string campaignType, string documentId, string documentStatus, string memberNumber, string campaignEndDate)
        {
            return new CampaignRegister
            {
                CustomerId = customerId,
                CampaignCode = campaignCode,
                CampaignId = campaignId,
                CreatedBy = createdBy,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                CampaignType = campaignType,
                DocumentId = documentId,
                DocumentStatus = documentStatus,
                MemberNumber = memberNumber,
                CampaignEndDate = campaignEndDate

            };
        }
    }
}
