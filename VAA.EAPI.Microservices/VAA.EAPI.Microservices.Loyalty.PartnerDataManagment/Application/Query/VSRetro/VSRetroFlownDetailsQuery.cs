using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagement.Application.Query.VSRetro
{
    public class VSRetroFlownDetailsQuery : IRequest<Result<Response<VSFlownDetailsResponse>>>
    {
        public string? TicketNumber { get; }
        public string? Pnr { get; }
        public string? FlightNumber { get; }

        public VSRetroFlownDetailsQuery(VSRetroSearchRequest retroSearchRequest)
        {
            ArgumentNullException.ThrowIfNull(retroSearchRequest);

            TicketNumber = retroSearchRequest.TicketNumber;
            Pnr = retroSearchRequest.Pnr;
            FlightNumber = retroSearchRequest.FlightNumber;
        }
    }
}
