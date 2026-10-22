using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces
{
    public interface IPnrLinkServiceHandler
    {
        Task<string> Handle(PnrLinkService request, CancellationToken cancellationToken);
    }
}
