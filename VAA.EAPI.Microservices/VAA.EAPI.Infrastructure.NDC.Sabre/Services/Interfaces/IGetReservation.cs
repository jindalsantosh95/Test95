using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IGetReservation
    {
        Task<GetReservationRS> GetReservationAsync(GetReservationOperationRequest getReservationRQ);
    }
}
