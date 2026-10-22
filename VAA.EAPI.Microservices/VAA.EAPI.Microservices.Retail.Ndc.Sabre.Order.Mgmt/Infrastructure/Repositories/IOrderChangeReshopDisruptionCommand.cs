using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public interface IOrderChangeReshopDisruptionCommand
    {
        Task<Result<Response<OrderDocument>>> Handle(OrderChangeRequestData request, CancellationToken cancellationToken);
    }
}
