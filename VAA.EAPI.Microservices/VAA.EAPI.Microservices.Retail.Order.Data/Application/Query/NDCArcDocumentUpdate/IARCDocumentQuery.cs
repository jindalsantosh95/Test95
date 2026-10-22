using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.NDCArcDocumentUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCArcDocumentUpdate
{
    public interface IARCDocumentQuery
    {
        Task<Response<ARCDocument>> Handle(string orderId, CancellationToken cancellationToken);
    }
}
