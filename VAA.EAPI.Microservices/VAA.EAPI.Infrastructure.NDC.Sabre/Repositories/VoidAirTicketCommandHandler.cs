using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.VoidAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class VoidAirTicketCommandHandler(IVoidAirTicket voidTicket) : IRequestHandler<VoidAirTicketCommand, VoidTicketRS>
    {
        private readonly IVoidAirTicket _voidTicket = voidTicket ?? throw new ArgumentNullException(nameof(voidTicket));

        public async Task<VoidTicketRS> Handle(VoidAirTicketCommand request, CancellationToken cancellationToken)
        {
            return await _voidTicket.VoidAirTicketAsync(request.VoidTicketRequest);
        }
    }
}
