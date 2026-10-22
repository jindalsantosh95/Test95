using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Mapper.OrderChange
{
    public static class OrderChangeQueryResponseMapping
    {
        public static Response<OrderDocument> OrderChangeQueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<OrderDocument>
            {
                Status = Status.OK,
                Value = Json.Deserialize<OrderDocument>(serviceResponse.JsonResponseData)
            };
        }
    }
}
