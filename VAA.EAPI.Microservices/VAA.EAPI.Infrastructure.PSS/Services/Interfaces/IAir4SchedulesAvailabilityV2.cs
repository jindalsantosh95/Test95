using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy.SchedulesAvailability;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface IAir4SchedulesAvailabilityV2
    {
        Task<RetrieveSpecificAvailabilityResponseType> RetrieveSpecificAvailability(RetrieveSpecificAvailabilityRequestType retrieveSpecificAvailabilityRequest);
    }
}
