using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Repositories
{
    public interface IUpdateAzureB2CProfileRepoCommandHandler
    {
        Task<bool> Handle(UpdateAzureB2CProfileRepoCommand req, CancellationToken cancellationToken);
    }

}
