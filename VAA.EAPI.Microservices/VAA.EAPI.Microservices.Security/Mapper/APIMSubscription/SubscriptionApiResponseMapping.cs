using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Security.Domain.APIMSubscription;


namespace VAA.EAPI.Microservices.Security.Mapper.APIMSubscription
{
    public class SubscriptionApiResponseMapping
    {
        public static Response<SubscriptionResponse> SubscriptionApiResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<SubscriptionResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<SubscriptionResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}

