using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command;

public interface IRegisterYoungFlyerCommandHandler
{
    Task<Response<B2CUserResponse>> Handle(RegisterYoungFlyerRequest request, CancellationToken cancellationToken);
}
