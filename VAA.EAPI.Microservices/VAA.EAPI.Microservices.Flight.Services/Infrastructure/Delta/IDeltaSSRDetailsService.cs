using System.Threading.Tasks;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta
{
    public interface IDeltaSSRDetailsService
    {
        Task<DeltaSSRResponse> RetrieveSSRDetails(string tripResevationID);
        Task<DeltaSSRResponse> ManageSSRDetailsAsync(SSRUpsertRequest ssrUpsertRequest, string tripResevationID);

    }
}
