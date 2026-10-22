using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.EntraEntities;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Interfaces
{
    public interface IRefreshTokenService
    {
        Task<string> RefreshToken(string refreshToken, IEnumerable<string> scopes, CancellationToken cancellationToken);
    }
}
