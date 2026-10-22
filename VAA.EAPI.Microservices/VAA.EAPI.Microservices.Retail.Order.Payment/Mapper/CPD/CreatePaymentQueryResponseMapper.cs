using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD
{
    public class CreatePaymentQueryResponseMapper
    {
        public static Response<PaymentResponse> QueryResponseMapper(ServiceResponse serviceResponse)
        {
            return new Response<PaymentResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<PaymentResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}
