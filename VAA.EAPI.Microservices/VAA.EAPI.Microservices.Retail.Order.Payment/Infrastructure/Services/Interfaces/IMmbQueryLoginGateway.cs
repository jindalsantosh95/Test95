using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.MmbQuery;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces
{
    public interface IMmbQueryLoginGateway
    {
        Task<MmbQueryLoginResult> LoginAsync(MmbQueryLoginRequest request, CancellationToken cancellationToken);
    }
}

