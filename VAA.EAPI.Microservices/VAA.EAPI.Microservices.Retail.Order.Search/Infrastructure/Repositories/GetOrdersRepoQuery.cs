using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.BookingDetails.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Infrastructure.Repositories
{
    public class OrdersRepoQuery : IRequest<IEnumerable<OrderDoc>>
    {
        public IEnumerable<string> Orders { get; set; }

        public OrdersRepoQuery(IEnumerable<string> order)
        {
            Orders = order;
        }
    }
}
