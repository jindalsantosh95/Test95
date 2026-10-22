using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetElectronicDocument;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestGetElectronicDocumentQuery : IGetElectronicDocument
    {
        public Task<GetElectronicDocumentRS> GetElectronicDocumentRQAsync(GetElectronicDocumentRQRequest request)
        {
            return Task.FromResult(TestDataProvider.GetXmlTestData<GetElectronicDocumentRS>("NDCOrderMgmtMS\\GetElectronicDocRS"));
        }
    }
}
