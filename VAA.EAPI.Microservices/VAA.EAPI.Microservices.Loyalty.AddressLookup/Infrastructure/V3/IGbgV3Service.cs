using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.V3
{
    public interface IGbgV3Service
    {
        Task<GbgResponse<FindResponse>> FindAsync(GbgV3FindRequest request, CancellationToken cancellationToken);
        Task<GbgResponse<RetrieveResponse>> RetrieveAsync(string providerAddressId, CancellationToken cancellationToken);
    }
}
