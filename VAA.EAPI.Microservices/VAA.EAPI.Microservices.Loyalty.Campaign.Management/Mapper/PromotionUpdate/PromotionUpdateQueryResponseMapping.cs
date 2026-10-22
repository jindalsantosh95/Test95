using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Mapper.PromotionUpdate
{
    public class PromotionUpdateQueryResponseMapping
    {
        public static Response<PromotionUpdateResponse> PromotionUpdateQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<PromotionUpdateResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<PromotionUpdateResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}
