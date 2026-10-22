using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.VoidAirTicket;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces
{
    public interface IVoidAirTicket
    {
        Task<VoidTicketRS> VoidAirTicketAsync(VoidTicketRQRequest request);
    }
}



