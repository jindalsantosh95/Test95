using CSharpFunctionalExtensions;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Application.OrderChangeNotif
{
    public interface IOrderChangeNotifCommand 
    {
        Task<Result<Response<string>>> Handle(OrderChangeNotifRQ request, CancellationToken cancellationToken);
    }
}
