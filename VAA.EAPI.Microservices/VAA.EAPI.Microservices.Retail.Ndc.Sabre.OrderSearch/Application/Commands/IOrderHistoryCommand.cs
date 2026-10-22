using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Application.Commands
{
    public interface IOrderHistoryCommand
    {
        Task<Result<Response<OrderDocument>>> Handle(OrderSearchRequest orderHistoryRequest, CancellationToken cancellationToken);
    }
}
