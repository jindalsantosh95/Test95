using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionCreate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.PromotionCreate
{
    public class PromotionCreateQueryResponseMapping
    {
        public static Response<PromotionCreateResponse> PromotionCreateQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<PromotionCreateResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<PromotionCreateResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}
