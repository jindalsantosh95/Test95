using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;


namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt
{
    public static class SplitPnrQueryResponseMapper
    {
        public static Response<OrderDocument> QueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<OrderDocument>
            {
                Status = Status.OK,
                Value = Json.Deserialize<OrderDocument>(serviceResponse.JsonResponseData)
            };
        }
    }
}
