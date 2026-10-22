using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Interface
{
    public interface IOrderListQuery
    {
        Task<IEnumerable<OrderListResponse>> Handle(IATA_OrderListRQ request, CancellationToken cancellationToken);
    }
}
