using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper
{
    public class OrderPaymentUpdateEventCommandResponseMapper
    {
        public static Response<string> OrderPaymentResponseMapper(string correlationid, Status status)
        {
            return new Response<string>
            {
                Status = status,
                HeaderParameters = null
            };
        }
    }
}
