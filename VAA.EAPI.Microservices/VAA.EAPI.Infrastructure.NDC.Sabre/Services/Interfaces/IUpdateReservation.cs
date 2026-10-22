using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IUpdateReservation
    {
        Task<UpdateReservationOperationResponse> UpdateReservationAsync(UpdateReservationOperationRequest updateReservationOperationRequest);
    }
}
