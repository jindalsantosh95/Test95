using MediatR;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingEmailReceiptQuery : IRequest<EmailReceiptResponse>
    {
        public string SessionId { get; set; }
        public string EmailId { get; set; }

        public TicketingEmailReceiptQuery()
        {
        }
    }
}
