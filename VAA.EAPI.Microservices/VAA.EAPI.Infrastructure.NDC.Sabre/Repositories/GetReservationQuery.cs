using MediatR;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class GetReservationQuery(GetReservationOperationRequest getReservationRQ) : IRequest<GetReservationRS>
    {
        public GetReservationOperationRequest GetReservationRQ { get; set; } = getReservationRQ;
    }
}
