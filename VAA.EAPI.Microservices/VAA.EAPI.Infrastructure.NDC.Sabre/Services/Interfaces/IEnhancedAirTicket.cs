using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IEnhancedAirTicket
    {
        Task<AirTicketRS> EnhancedAirTicketRQAsync(AirTicketRQRequest enhancedAirTicketRQ);
    }
}
