using VAA.EAPI.Microservices.CRM.Email.Constants;

namespace VAA.EAPI.Microservices.CRM.Email.Domain
{
    public class SendOrderEmailRequest
    {
        public RequestType RequestType { get; set; }
        public OrderDocument OrderDocument { get; set; }
    }
}
