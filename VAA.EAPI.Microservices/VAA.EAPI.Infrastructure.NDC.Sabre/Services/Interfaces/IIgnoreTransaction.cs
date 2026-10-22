using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.IgnoreTransaction;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IIgnoreTransaction
    {
        Task<IgnoreTransactionRQResponse> IgnoreTransactionAsync(IgnoreTransactionRQRequest ignoreTransactionRQ);
    }
}
