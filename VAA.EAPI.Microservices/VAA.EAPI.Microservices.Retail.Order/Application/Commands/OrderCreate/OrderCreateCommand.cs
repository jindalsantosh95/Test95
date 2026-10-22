using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Commands.OrderCreate
{
    public class OrderCreateCommand : IRequest<Result<Response<string>>>
    {
        public OrderCreateRequest Request { get; set; }

        public OrderCreateCommand(OrderCreateRequest request)
        {
            Request = request;
        }
    }
}
