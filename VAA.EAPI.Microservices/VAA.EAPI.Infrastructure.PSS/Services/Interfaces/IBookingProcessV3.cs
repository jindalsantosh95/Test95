using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Proxy;

namespace VAA.EAPI.Infrastructure.PSS.Services.Interfaces
{
    public interface IBookingProcessV3
    {
        Task<PerformRevenueResponseType> PerformRevenue(PerformRevenueRequestType performRevenueRequest);
    }
}
