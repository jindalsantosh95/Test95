using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;
using VAA.EAPI.NDC.Order.Domain.OrderListRS;

namespace VAA.EAPI.NDC.Order.Application.OrderList
{
    public interface IOrderListCommand
    {
        Task<Result<Response<IATA_OrderListRS>>> Handle(IATA_OrderListRQ request, string subscriptionKey, CancellationToken cancellationToken);
    }
}
