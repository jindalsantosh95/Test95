using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRS;

namespace VAA.EAPI.NDC.Order.Application.OrderHistory
{
    public interface IOrderHistoryQuery
    {
        Task<Result<Response<IATA_OrderHistoryRS>>> Handle(IATA_OrderHistoryRQ request, string subscriptionKey, CancellationToken cancellationToken);

    }
}
