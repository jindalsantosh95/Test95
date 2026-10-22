using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.OrderCreate
{
    public class OrderCreateServiceCommand : IRequest<Result<Response<OrderResponse>>>
    {
        public OrderCreateRequest Request { get; set; }
    }
}
