using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Mapper
{
    public class OrderPaymentResponseMapper
    {
        public static OrderPaymentResponse ResponseMapper(OrderPaymentStatusRequest serviceResponse, string correlationId)
        {
            return new OrderPaymentResponse
            {
                CorrelationId = correlationId,
                Status = serviceResponse.Status,
                Data = serviceResponse.PayLoad
            };
        }
    }
}