using MediatR;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PNRRetrieveRepoQuery : IRequest<RetrieveResponseType>
    {
        public RetrieveRequestType RetrieveRequestType { get; set; }

        public PNRRetrieveRepoQuery(RetrieveRequestType retrieveRequestType)
        {
            RetrieveRequestType = retrieveRequestType;
        }
    }
}
