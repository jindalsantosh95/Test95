using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt
{
    public interface INdcQueueMgmtFetchDuplicatePnrsCommand
    {
        Task<IReadOnlyCollection<string>> Handle(OrderChangeDuplicatePNRNotifRQ request, CancellationToken cancellationToken);
    }
}

