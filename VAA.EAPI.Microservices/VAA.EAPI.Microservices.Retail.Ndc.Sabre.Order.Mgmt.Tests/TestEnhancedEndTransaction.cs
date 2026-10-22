using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Tests
{
    public class TestEnhancedEndTransaction : IEnhancedEndTransaction
    {
        public Task<EnhancedEndTransactionRS> EnhancedEndTransactionRQAsync(EnhancedEndTransactionRQRequest enhancedEndTransactionRQ)
        {
           return Task.FromResult(new EnhancedEndTransactionRS
            {
                ApplicationResults = new()
                {
                    status = CompletionCodes.Complete
                },
           });
        }
    }
}
