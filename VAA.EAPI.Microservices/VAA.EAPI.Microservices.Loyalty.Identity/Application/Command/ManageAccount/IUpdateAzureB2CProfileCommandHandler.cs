using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Application.Command.ManageAccount
{
    public interface IUpdateAzureB2CProfileCommandHandler
    {
        Task<Result<bool>> Handle(UpdateAzureB2CProfileCommand request, CancellationToken cancellationToken);
    }
}
