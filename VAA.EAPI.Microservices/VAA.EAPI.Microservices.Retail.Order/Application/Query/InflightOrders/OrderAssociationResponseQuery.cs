using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.Microservices.Retail.Order.Application.Query.InflightOrders
{
    public class OrderAssociationResponseQuery : IRequest<Result<Response<string>>>
    {
        public string Correlationid { get; set; }
        public string Type { get; set; }
    }
}
