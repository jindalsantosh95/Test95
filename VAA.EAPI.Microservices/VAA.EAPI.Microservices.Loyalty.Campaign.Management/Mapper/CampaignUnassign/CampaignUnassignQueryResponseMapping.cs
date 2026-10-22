using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUnassign;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignUnassign
{
    public class CampaignUnassignQueryResponseMapping
    {
        public static Response<CampaignUnassignResponse> CampaignUnassignQueryResponseMapper(ServiceResponse serviceResponse, string requestId)
        {

            CampaignUnassignServiceResponseData serviceResponseData = Json.Deserialize<CampaignUnassignServiceResponseData>(serviceResponse.JsonResponseData);

            if (serviceResponseData.Value.Status == Status.OK)
                return new Response<CampaignUnassignResponse>()
                {
                    Status = Status.OK,
                    Value = new CampaignUnassignResponse()
                    {
                        Status = Status.OK,
                        RequestId = requestId,
                        Value = Constants.Application.campaignUnassignSuccessMessage
                    }
                };
            else
                return new Response<CampaignUnassignResponse>()
                {
                    Status = Status.OK,
                    Value = new CampaignUnassignResponse()
                    {
                        Status = Status.BusinessException,
                        Errors = serviceResponseData.Value.Errors,
                        Value = Constants.Application.campaignUnassignFailureMessage
                    }
                };
        }
    }
}