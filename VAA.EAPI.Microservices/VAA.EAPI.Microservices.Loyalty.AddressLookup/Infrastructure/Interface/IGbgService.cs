using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface
{
    public interface IGbgService
    {
        Task<GbgResponse<FindResponse>> Find(string addressText, string countryCode, CancellationToken cancellationToken);
        Task<GbgResponse<RetrieveResponse>> Retrieve(string addressId, CancellationToken cancellationToken);        
    }
}
