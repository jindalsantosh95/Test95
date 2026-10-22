using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.OrderList;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.OrderList;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.NDCOrderList.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCOrderList
{
    public class OrderListQueryHandler : IOrderListQuery
    {
        private readonly IOrderListRepoQuery _orderListRepoQuery;

        public OrderListQueryHandler(IOrderListRepoQuery orderListRepoQuery)
        {
            _orderListRepoQuery = orderListRepoQuery ?? throw new ArgumentNullException(nameof(orderListRepoQuery));
        }

        public async Task<Response<IEnumerable<OrderListResponse>>> Handle(NameValueCollection query, CancellationToken cancellationToken)
        {
            var cosmosQuery = OrderListQueryMapper.CosmosQueryMapper(query);
            var cosmosDocs = await _orderListRepoQuery.Handle(cosmosQuery, cancellationToken);
            var result = Json.Deserialize<IEnumerable<OrderListResponse>>(Json.Serialize(cosmosDocs));

            return new Response<IEnumerable<OrderListResponse>> { Status = Status.OK, Value = result };
        }
    }
}
