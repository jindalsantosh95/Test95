using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.VoidAirTicket;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class VoidAirTicketCommand : IRequest<VoidTicketRS>
    {
        public VoidTicketRQRequest VoidTicketRequest { get; set; }
    }
}


