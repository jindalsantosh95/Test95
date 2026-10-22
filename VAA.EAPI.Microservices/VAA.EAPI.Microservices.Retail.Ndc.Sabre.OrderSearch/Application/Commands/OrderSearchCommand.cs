using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands
{
    public class OrderSearchCommand : IRequest<Result<Response<OrderDocument>>>
    {
        public OrderSearchRequest OrderSearchRequest { get; set; }

        public OrderSearchCommand(OrderSearchRequest orderCreateRequest)
        {
            OrderSearchRequest = orderCreateRequest;
        }
    }
}
