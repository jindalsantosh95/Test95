using MediatR;
using VAA.EAPI.Domain.Models.B2CGraph;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Repositories.InflightOrders
{
    public class B2CGraphRepoQuery : IRequest<B2CGraphResponse>
    {
        public string CustomerId { get; set; }

        public B2CGraphRepoQuery(string customerId)
        {
            CustomerId = customerId;
        }
    }
}
