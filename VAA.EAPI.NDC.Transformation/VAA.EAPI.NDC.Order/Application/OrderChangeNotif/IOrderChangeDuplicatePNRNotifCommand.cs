using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Application.OrderChangeNotif
{
    public interface IOrderChangeDuplicatePNRNotifCommand
    {
        Task<Result<Response<string>>> Handle( OrderChangeDuplicatePNRNotifRQ request, CancellationToken cancellationToken);
    }
}