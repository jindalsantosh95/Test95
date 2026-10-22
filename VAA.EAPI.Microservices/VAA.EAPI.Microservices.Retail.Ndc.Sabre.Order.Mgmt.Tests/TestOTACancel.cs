using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.OTACancel;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestOTACancel : IOTACancelLLS
    {
        public Task<OTA_CancelRQResponse> OTACancelLLSAsync(OTA_CancelRQRequest request)
        {
            return Task.FromResult(new OTA_CancelRQResponse
            {
                OTA_CancelRS = new()
                {
                    ApplicationResults = new()
                    {
                        status = CompletionCodes.Complete
                    },
                }
            });
        }
    }
}
