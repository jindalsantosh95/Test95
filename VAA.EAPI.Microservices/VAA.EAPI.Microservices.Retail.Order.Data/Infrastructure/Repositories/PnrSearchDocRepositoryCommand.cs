using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories
{
    public class PnrSearchDocRepositoryCommand : IRequest<Unit>
    {
        public OrderCreateRequestData Req { get; set; }

        public PnrSearchDocRepositoryCommand(OrderCreateRequestData req)
        {
            Req = req;
        }
    }
}
