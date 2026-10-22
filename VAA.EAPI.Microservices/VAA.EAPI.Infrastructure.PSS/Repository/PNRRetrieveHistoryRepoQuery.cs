using MediatR;
using VAA.EAPI.Infrastructure.PSS.Models;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PNRRetrieveHistoryRepoQuery : IRequest<PNRCommonV6RetrieveResponse>
    {
        public string RLOC { get; set; }

        public PNRRetrieveHistoryRepoQuery(string rLoc)
        {
            RLOC = rLoc;
        }
    }
}
