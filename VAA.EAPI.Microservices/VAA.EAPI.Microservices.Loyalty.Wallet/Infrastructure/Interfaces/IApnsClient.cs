using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Wallet.Domain.Apns;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Interfaces
{
    public interface IApnsClient
    {
        Task<ApnsResponse> SendAsync(ApplePush push, CancellationToken ct = default);
    }
}
