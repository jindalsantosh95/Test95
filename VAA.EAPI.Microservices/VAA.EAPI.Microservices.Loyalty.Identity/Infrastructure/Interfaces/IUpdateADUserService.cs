using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IUpdateAdUserService
    {
        Task<B2CUserResponse> UpdateAdUser(string api, string b2CServiceQuery, string content, HttpMethod httpMethod, CancellationToken cancellationToken = default);

        Task<bool> UpdateMFADetailsFromGraphApi(string api, string content, CancellationToken cancellationToken = default);
    }
}
