using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TravelItineraryDivideLLS;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface ITravelItineraryDivideCommandLLS
    {
        Task<TravelItineraryDivideRQResponse> TravelItineraryDivideRQAsync(TravelItineraryDivideRQRequest request);
    }
}
