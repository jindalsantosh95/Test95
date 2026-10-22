using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class EnhancedAirTicketCommand : IRequest<AirTicketRS>
    {
        public AirTicketRQRequest EnhancedAirTicketRequest { get; set; }
    }
}