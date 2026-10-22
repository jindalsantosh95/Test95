using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Models.B2CGraph;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IDeleteB2CAccountService
    {
        Task<B2CUserResponse> DeleteB2CAccount(string customerId, CancellationToken cancellationToken = default);
    }
}
