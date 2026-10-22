using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.RegisterToB2C;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.AccountAccess.Interfaces
{
    public interface IPatchRegisteredUserHandler
    {
        Task Handle(PatchRegisteredUserRequest request, CancellationToken cancellationToken);
    }
}
