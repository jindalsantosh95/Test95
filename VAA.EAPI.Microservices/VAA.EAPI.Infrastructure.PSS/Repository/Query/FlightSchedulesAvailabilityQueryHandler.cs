using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Query
{
    public class FlightSchedulesAvailabilityQueryHandler : IRequestHandler<FlightSchedulesAvailabilityQuery, Segment>
    {
        private readonly IAir4SchedulesAvailabilityV2 _iAir4SchedulesAvailabilityV2;

        public FlightSchedulesAvailabilityQueryHandler(IAir4SchedulesAvailabilityV2 iAir4SchedulesAvailabilityV2)
        {
            _iAir4SchedulesAvailabilityV2 = iAir4SchedulesAvailabilityV2 ?? throw new ArgumentNullException(nameof(iAir4SchedulesAvailabilityV2));
        }

        public async Task<Segment> Handle(FlightSchedulesAvailabilityQuery request, CancellationToken cancellationToken)
        {
            var map = SchedulesAvailabilityMapper.RetrieveSpecificAvailabilityRequestMapping(request);
            var resp = await _iAir4SchedulesAvailabilityV2.RetrieveSpecificAvailability(map);


            return SchedulesAvailabilityMapper.RetrieveSpecificAvailabilityResponseMapping(request, resp);
        }
    }
}
