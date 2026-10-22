using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedEndTransaction;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IEnhancedEndTransaction
    {
        Task<EnhancedEndTransactionRS> EnhancedEndTransactionRQAsync(EnhancedEndTransactionRQRequest enhancedEndTransactionRQ);
    }
}
