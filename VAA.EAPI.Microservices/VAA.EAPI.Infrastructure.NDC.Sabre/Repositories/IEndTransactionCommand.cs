using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EndTransaction;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public interface IEndTransactionCommand
    {
        Task<EndTransactionRS> Handle(EndTransactionRQRequest endTransactionRQRequest, CancellationToken cancellationToken);
    }
}
