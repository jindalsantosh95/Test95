using VAA.EAPI.NDC.Order.Constants;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.EmailMS
{
    public class SendOrderEmailRequest
    {
        public RequestType RequestType {  get; set; }
        public OrderDocument OrderDocument { get; set; }
    }
}
