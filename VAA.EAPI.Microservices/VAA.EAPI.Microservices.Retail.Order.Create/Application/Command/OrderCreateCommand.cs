using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Create.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Application.Command
{
    public class OrderCreateCommand : IRequest<Result<Response<string>>>
    {
        public OrderCreateRequest OrderCreateRequest { get; set; }

        public OrderCreateCommand(OrderCreateRequest orderCreateRequest)
        {
            OrderCreateRequest = orderCreateRequest;
        }
    }
}
