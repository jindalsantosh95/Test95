using MediatR;
using VAA.EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class TicketingRetrieveRepoQuery : IRequest<RetrieveResponseType>
    {
        public RetrieveRequestType RetrieveRequestType { get; set; }
        public TicketingRetrieveRepoQuery(RetrieveRequestType retrieveRequestType)
        {
            RetrieveRequestType = retrieveRequestType;
        }
    }
}
