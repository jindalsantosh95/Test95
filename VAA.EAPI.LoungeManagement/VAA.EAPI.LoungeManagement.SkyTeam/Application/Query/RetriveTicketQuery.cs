using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.LoungeManagement.SkyTeam.Domain;

namespace VAA.EAPI.LoungeManagement.SkyTeam.Application.Query
{
    public class RetriveTicketQuery : IRequest<Response<RetrieveTicketDetailsResponse>>
    {
        public RetrieveTicketDetailsRequest RetrieveTicketDetailsRequest { get; set; }
    }
}
