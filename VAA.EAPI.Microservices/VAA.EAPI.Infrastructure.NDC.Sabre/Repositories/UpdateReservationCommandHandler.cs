using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.UpdateReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Repositories
{
    public class UpdateReservationCommandHandler(IUpdateReservation updateReservation) : IUpdateReservationCommand
    {
        private readonly IUpdateReservation _updateReservation = updateReservation ?? throw new ArgumentNullException(nameof(updateReservation));

        public async Task<UpdateReservationOperationResponse> Handle(UpdateReservationOperationRequest request, CancellationToken cancellationToken)
        {
            return await _updateReservation.UpdateReservationAsync(request);
        }
    }
}
