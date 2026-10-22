using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Mapper
{
    public class TransactionResponseQueryMapper
    {
        public static Response<TransactionQueryResponse> ResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<TransactionQueryResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<TransactionQueryResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}