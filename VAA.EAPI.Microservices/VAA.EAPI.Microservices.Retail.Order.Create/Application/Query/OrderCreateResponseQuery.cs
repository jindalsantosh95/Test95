using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Create.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Create.Application.Query
{
    public class OrderCreateResponseQuery : IRequest<Result<Response<OrderResponse>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
