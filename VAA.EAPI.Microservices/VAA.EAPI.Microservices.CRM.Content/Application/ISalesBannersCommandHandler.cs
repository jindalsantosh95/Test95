using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.CRM.Content.Domain.SalesBanner;

namespace VAA.EAPI.Microservices.CRM.Content.Application
{
    public interface ISalesBannersCommandHandler
    {
        Task<Dictionary<string, IEnumerable<SalesBanner>>> Handle(SalesBannersCommand request, CancellationToken cancellationToken);
    }
}