using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
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
