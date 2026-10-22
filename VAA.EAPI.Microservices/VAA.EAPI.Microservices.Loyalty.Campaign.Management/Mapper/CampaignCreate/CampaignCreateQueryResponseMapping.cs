using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.CampaignCreate
{
    public class CampaignCreateQueryResponseMapping
    {
        public static Response<CampaignCreateResponse> CampaignCreateQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<CampaignCreateResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<CampaignCreateResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}
