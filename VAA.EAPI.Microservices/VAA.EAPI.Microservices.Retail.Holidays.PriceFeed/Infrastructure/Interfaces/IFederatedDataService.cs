using VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Holidays.PriceFeed.Infrastructure.Interfaces
{
    public interface IFederatedDataService
    {
        Task<FederatedDataAPIResponse> GetFederatedDataAsync(string destination);
    }
}
