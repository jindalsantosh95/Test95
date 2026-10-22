using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt
{
    public interface INdcQueueMgmtRemoveDuplicatePnrsCommand
    {
        Task<bool> Handle(OrderChangeDuplicatePNRNotifRQ request, string pnr, CancellationToken cancellationToken);
    }
}

