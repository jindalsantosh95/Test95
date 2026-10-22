using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.NDCOrderStore
{
    public class OrderDocQuery : IRequest<Response<OrderDocument>>
    {
        public string OrderId { get; set; }
    }
}
