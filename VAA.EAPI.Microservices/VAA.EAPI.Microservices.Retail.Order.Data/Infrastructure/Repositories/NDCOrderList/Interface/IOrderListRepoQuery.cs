using Azure;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Model.OrderList;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.NDCOrderList.Interface
{
    public interface IOrderListRepoQuery
    {
        Task<IEnumerable<object>> Handle(CosmosQueryParameters CosmosQuery, CancellationToken cancellationToken);
    }
}
