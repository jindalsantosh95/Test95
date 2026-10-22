using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Query.OrderCreate
{
    public class OrderCreateResponseQuery : IRequest<Result<Response<OrderResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
