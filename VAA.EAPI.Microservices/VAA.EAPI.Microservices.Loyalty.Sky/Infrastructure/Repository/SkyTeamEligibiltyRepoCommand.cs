using MediatR;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Infrastructure.Repository
{
    public class SkyTeamEligibiltyRepoCommand : IRequest<CheckEligibilityResponse>
    {
        public TicketInfo TicketInfo { get; set; }

        public SkyTeamEligibiltyRepoCommand(TicketInfo ticketInfo)
        {
            TicketInfo = ticketInfo;
        }
    }
}
