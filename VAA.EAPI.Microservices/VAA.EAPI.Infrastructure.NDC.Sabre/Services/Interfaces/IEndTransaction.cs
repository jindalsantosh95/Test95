using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EndTransaction;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IEndTransaction
    {
        Task<EndTransactionRS>EndTransactionRQAsync(EndTransactionRQRequest endTransactionRQ);
    }
}
