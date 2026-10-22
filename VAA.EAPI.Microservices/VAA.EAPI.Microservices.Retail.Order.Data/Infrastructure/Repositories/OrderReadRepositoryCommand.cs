using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class OrderReadRepositoryCommand : IRequest<PnrSearch>
    {
        public OrderReadRequestData Req { get; set; }

        public OrderReadRepositoryCommand(OrderReadRequestData req)
        {
            Req = req;
        }
    }
}
