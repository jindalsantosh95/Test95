using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestEnhancedAirTicket : IEnhancedAirTicket
    {
        public Task<AirTicketRS> EnhancedAirTicketRQAsync(AirTicketRQRequest enhancedAirTicketRQ)
        {
            return Task.FromResult(TestDataProvider.GetXmlTestData<AirTicketRS>("NDCOrderMgmtMS\\EnhancedAirTicketRS"));
        }
    }
}
