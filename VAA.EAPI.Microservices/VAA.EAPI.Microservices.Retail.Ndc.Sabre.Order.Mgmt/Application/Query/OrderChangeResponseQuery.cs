using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Query
{
    public class OrderChangeResponseQuery : IRequest<Result<Response<OrderDocument>>>
    {
        public string CorrelationId { get; set; }
        public string Type { get; set; }
    }
}
