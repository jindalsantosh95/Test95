using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Infrastructure.Repositories
{
    public class OrderCreateRepoCommand : IRequest<Result<Response<GetReservationRS>>>
    {
        public OrderCreateRequest OrderCreateRequest { get; set; }
    }
}
