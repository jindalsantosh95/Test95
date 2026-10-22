using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper
{
    public class CampaignCommandQueryResponseMapping
    {
        public static Response<CampaignCommandResponse> CampaignCommandQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<CampaignCommandResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<CampaignCommandResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}
