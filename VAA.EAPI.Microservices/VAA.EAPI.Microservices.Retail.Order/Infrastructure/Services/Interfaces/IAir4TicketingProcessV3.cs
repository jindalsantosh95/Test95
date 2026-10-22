using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.TicketingProcessV3Proxy;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Services.Interfaces
{
    public interface IAir4TicketingProcessV3
    {
        Task<RepriceResponseType> Reprice(RepriceRequestType repriceRequestType);
        Task<PerformRevenueResponseType> PerformRevenue(PerformRevenueRequestType performRevenueRequestType);

    }
}
