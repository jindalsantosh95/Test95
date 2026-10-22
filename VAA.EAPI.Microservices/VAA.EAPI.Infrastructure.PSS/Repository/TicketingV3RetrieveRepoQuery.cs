using MediatR;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingV3RetrieveRepoQuery : IRequest<TicketingV3RetrieveResponse>
    {
        public RetrieveRequestType RetrieveRequestType { get; set; }

        public TicketingV3RetrieveRepoQuery(RetrieveRequestType retrieveRequestType)
        {
            RetrieveRequestType = retrieveRequestType;
        }
    }
}
