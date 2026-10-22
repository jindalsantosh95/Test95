using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class OrderDocumentRepoQuery : IRequest<List<OrderDocument>>
    {
        public string OrderId { get; set; }
    }
}
