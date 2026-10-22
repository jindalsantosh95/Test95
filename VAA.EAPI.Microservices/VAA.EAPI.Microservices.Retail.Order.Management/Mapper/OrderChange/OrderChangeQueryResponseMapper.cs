using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class OrderChangeQueryResponseMapping
    {
        public static Response<OrderChangeResponse> OrderChangeQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<OrderChangeResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<OrderChangeResponse>(serviceResponse.JsonResponseData)
            };
        }

    }
}
