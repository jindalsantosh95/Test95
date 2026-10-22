using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.SeatAvailability;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface ISeatAvailabilityV8
    {
        Task<RetrievePassengerMapResponse> RetrievePassengerMap(RetrievePassengerMapRequestType retrievePassengerMapRequest);
    }
}
