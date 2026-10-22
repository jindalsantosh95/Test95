using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Infrastructure.Model.EmailMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.EmailMS
{
    public static class SendOrderEmailRequestMapper
    {
        public static SendOrderEmailRequest MapEmailRequest(RequestType type, OrderDocument orderDocument)
        {
            return new SendOrderEmailRequest()
            {
                RequestType = type,
                OrderDocument = orderDocument
            };
        }
    }
}
