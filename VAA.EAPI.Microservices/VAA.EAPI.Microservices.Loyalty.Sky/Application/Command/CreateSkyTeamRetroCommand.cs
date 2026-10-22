using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sky.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Sky.Application.Command
{
    public class CreateSkyTeamRetroCommand : IRequest<Response<CheckEligibilityResponse>>
    {
        public TicketInfo TicketInfo { get; set; }

        public CreateSkyTeamRetroCommand(TicketInfo ticketInfo)
        {
            TicketInfo = ticketInfo;
        }
    }
}
