using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.FlyingClubToken;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.FlyingClubToken
{
    public interface IFlyingClubTokenHandler
    {
        Task<Result<Response<FlyingClubTokenResponse>>> HandleAsync(FlyingClubTokenRequest request, CancellationToken cancellationToken);
    }
}
