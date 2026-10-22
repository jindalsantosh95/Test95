using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.OrderData;

namespace VAA.Facade.NDC.IFGFacade.Infrastructure.Services.OrderDataMS
{
    public interface IOrderDataQuery
    {
        Task<OrderRS> Handle(string orderId, CancellationToken cancellationToken);
    }
}
