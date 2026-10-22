using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.TravelItineraryDivideLLS;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class TravelItineraryDivideLLSCommandHandler (ITravelItineraryDivideCommandLLS travelItineraryDivide) : ITravelItineraryDivideLLSCommand
    {
        private readonly ITravelItineraryDivideCommandLLS _travelItineraryDivideCommandLLS = travelItineraryDivide ?? throw new ArgumentNullException(nameof(travelItineraryDivide));
               
        public async Task<TravelItineraryDivideRQResponse> Handle(TravelItineraryDivideRQRequest request, CancellationToken cancellationToken)
        {
            return await _travelItineraryDivideCommandLLS.TravelItineraryDivideRQAsync(request);
        }
        
    }
}
