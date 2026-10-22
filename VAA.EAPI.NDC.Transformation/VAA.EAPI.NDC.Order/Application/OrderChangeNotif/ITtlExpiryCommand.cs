using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Order.Domain;

namespace VAA.EAPI.NDC.Order.Application.OrderChangeNotif
{
    public interface ITtlExpiryCommand
    {
        Task Handle(OcnTtlExpiryRQ request, string requestType, CancellationToken cancellationToken);
    }
}