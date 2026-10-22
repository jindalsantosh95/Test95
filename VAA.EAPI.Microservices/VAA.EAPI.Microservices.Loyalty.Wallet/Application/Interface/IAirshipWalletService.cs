using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Application.Interface
{
    public interface IAirshipWalletService
    {
        Task<FlightResponse> PostFlightAsync(string flightExtId, FlightRequest flightRequest);
        Task<FlightResponse> PatchFlightAsync(string flightExtId, FlightRequest flightRequest);
        Task<FlightResponse> GetFlightAsync(string flightExtId);
        Task<AirshipBoardingPassResponse> PostBoardingPassAsync(AirshipBoardingPassRequest flightRequest);
    }
}
