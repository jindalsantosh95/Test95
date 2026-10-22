using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Commands.NDCOrderStore
{
    public class OrderDocUpdateCommand : IRequest<Result<Response<string>>>
    {
        public OrderDocument OrderDocument { get; set; }
    }
}
