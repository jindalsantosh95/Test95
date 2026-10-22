using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.EnhancedAirTicket;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class EnhancedAirTicketCommandHandler : IRequestHandler<EnhancedAirTicketCommand, AirTicketRS>
    {
        private readonly IEnhancedAirTicket _enhancedAirTicket;

        public EnhancedAirTicketCommandHandler(IEnhancedAirTicket enhancedAirTicket)
        {
            _enhancedAirTicket = enhancedAirTicket ?? throw new ArgumentNullException(nameof(enhancedAirTicket));
        }

        public async Task<AirTicketRS> Handle(EnhancedAirTicketCommand request, CancellationToken cancellationToken)
        {
            return await _enhancedAirTicket.EnhancedAirTicketRQAsync(request.EnhancedAirTicketRequest);
        }
    }

}