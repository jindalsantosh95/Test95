using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SessionClose;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestSessionClose : ISessionClose
    {
        public Task<SessionCloseRQResponse> SessionCloseAsync(SessionCloseRQRequest sessionCloseRQ)
        {
            return Task.FromResult(new SessionCloseRQResponse() { SessionCloseRS = new() { status = "Approved "} });
        }
    }
}
