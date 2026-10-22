using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class OrderChangeRepoCommand : IRequest<Result<Response<OrderDocument>>>
    {
        public OrderChangeRequestData orderChangeRequestData { get; set; }
    }
}
