using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.CRM.Domain;

namespace VAA.EAPI.Microservices.CRM.Mapper
{
    public class PaymentUpdateQueryResponseMapper
    {
        public static Response<PaymentResponse> MapToPaymentUpdateResponse(ServiceResponse serviceResponse)
        {
            return new Response<PaymentResponse>
            {
                Status = Status.OK,
                Value = Json.Deserialize<PaymentResponse>(serviceResponse.JsonResponseData)
            };
        }
    }
}
