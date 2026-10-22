using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS
{
    public interface IARCOrderDataMSQuery
    {
        Task<ARCDocumentResponse> Handle(string orderId, CancellationToken cancellationToken);
    }
}
