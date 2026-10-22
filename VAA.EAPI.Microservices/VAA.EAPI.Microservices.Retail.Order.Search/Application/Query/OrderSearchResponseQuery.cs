using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query
{
    public class OrderSearchResponseQuery : IRequest<Result<Response<OrderSearchResponse>>>
    {
        public OrderSearchRequest OrderSearchRequest { get; set; }
        public HttpRequestData HttpRequest { get; set; }
    }
}
