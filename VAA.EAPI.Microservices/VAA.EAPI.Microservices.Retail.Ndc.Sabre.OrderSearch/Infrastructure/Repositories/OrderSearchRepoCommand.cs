using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories
{
    public class OrderSearchRepoCommand : IRequest<Result<Response<GetReservationRS>>>
    {
        public OrderSearchRequest OrderSearchRequest { get; set; }
    }
}
