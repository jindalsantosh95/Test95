using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public interface IArcDocumentRepoQuery
    {
        Task<ARCDocument> Handle( string orderId,CancellationToken cancellationToken);
    }
}
