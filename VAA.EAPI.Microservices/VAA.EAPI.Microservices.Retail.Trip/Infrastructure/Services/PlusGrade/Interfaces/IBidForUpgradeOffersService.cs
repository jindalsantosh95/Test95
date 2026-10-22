using VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.PlusGrade;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Services.PlusGrade.Interfaces
{
    public interface IBidForUpgradeOffersService
    {
        Task<BidForUpgradeResponse> GetBidForUpgradeOffersAsync(BidForUpgradeRequest bidForUpgradeRequest);
    }
}
