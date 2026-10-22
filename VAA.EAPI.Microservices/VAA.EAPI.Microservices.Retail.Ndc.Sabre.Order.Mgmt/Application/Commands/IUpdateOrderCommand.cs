using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Application.Commands
{
    public interface IUpdateOrderCommand
    {
        Task<Result<Response<string>>> Handle(UpdateOrderRequest orderHistoryRequest, CancellationToken cancellationToken);

    }
}
