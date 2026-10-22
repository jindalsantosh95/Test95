using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.OrderList;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCOrderList
{
    public interface IOrderListQuery
    {
        Task<Response<IEnumerable<OrderListResponse>>> Handle(NameValueCollection query, CancellationToken cancellationToken);
    }
}
