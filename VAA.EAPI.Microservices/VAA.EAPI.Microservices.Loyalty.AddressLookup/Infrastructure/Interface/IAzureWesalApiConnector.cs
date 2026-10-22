using System.Threading.Tasks;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface
{
    public interface IAzureWesalApiConnector
    {
        public Task<string> GetAddressDetails(string request);
    }
}
