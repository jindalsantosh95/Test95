using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TravelItineraryDivideLLS;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public interface ITravelItineraryDivideLLSCommand
    {
        Task<TravelItineraryDivideRQResponse> Handle(TravelItineraryDivideRQRequest travelItineraryDivideRQRequest, CancellationToken cancellationToken);
    }
}
