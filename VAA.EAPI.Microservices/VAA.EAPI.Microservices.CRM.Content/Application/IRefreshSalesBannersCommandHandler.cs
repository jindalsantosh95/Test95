using System.Threading;
using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.CRM.Content.Application
{
    public interface IRefreshSalesBannersCommandHandler
    {
        Task Handle(CancellationToken cancellationToken);
    }
}