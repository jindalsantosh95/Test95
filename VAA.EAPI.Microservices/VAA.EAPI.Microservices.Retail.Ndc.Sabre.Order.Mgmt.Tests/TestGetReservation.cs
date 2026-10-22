using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestGetReservation : IGetReservation
    {
        public Task<GetReservationRS> GetReservationAsync(GetReservationOperationRequest getReservationRQ)
        {
            var response = TestDataProvider.GetXmlTestData<GetReservationRS>("NDCOrderMgmtMS\\GetReservRS");
            return Task.FromResult(response);
        }
    }
}
