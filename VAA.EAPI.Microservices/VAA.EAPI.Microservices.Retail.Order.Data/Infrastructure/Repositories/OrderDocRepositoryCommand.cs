using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class OrderDocRepositoryCommand : IRequest<Unit>
    {
        public OrderCreateRequestData Req { get; set; }

        public OrderDocRepositoryCommand(OrderCreateRequestData req)
        {
            Req = req;
        }
    }
}
