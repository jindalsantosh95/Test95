using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public interface IUpdateReservationCommand
    {
        Task<UpdateReservationOperationResponse> Handle(UpdateReservationOperationRequest updateReservationOperationRequest, CancellationToken cancellationToken);
    }
}
