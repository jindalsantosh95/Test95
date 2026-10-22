using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class GetReservationQueryHandler(IGetReservation getReservation) : IRequestHandler<GetReservationQuery, GetReservationRS>
    {
        private readonly IGetReservation _getReservation = getReservation ?? throw new ArgumentNullException(nameof(getReservation));

        public async Task<GetReservationRS> Handle(GetReservationQuery request, CancellationToken cancellationToken)
        {
            return await _getReservation.GetReservationAsync(request.GetReservationRQ);
        }
    }
}
