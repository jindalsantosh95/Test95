using MediatR;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class PnrTeardownContextRepoCommand : IRequest<TeardownContextResponseType>
    {
        public string SessionId { get; set; }
        public bool IsOrderEmail { get; set; } = false;
    }
}
