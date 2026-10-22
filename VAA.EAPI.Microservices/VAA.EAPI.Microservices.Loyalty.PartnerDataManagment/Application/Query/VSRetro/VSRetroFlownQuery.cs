using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Application.Query.VSRetro
{
    public class VSRetroFlownQuery : IRequest<Result<Response<VSFlownDetailsResponse>>>
    {
        public string TicketNumber { get; set; }

        public VSRetroFlownQuery(string ticketNumber)
        {
            TicketNumber = ticketNumber ?? throw new ArgumentNullException(nameof(ticketNumber));
        }
    }
}
