using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository
{
    public class SeatAvailabilityQueryHandler : IRequestHandler<SeatAvailabilityQuery, Entities.SeatAvailability.RetrievePassengerMapResponse>
    {
        private readonly ISeatAvailabilityV8 _iSeatAvailabilityV8;
        public SeatAvailabilityQueryHandler(ISeatAvailabilityV8 iSeatAvailabilityV8)
        {
            _iSeatAvailabilityV8 = iSeatAvailabilityV8 ?? throw new ArgumentNullException(nameof(iSeatAvailabilityV8));

        }
        public async Task<Entities.SeatAvailability.RetrievePassengerMapResponse> Handle(SeatAvailabilityQuery request, CancellationToken cancellationToken)
        {
            return SeatAvailabilityMapper.RetrievePassengerMapResponseMapping(await _iSeatAvailabilityV8.RetrievePassengerMap(SeatAvailabilityMapper.RetrievePassengerMapRequestMapping(request)));
        }
    }
}
