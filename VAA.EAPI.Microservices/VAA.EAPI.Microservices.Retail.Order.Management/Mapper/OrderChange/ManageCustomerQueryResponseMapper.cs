using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class ManageCustomerQueryResponseMapper
    {
        public static Response<OrderView> QueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<OrderView>
            {
                Status = Status.OK,
                Value = Json.Deserialize<OrderView>(serviceResponse.JsonResponseData)
            };
        }
    }
}
