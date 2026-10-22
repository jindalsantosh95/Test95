using System;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class TransportDestinationDetails
    {
        public DateTime ScheduledArrivalLocalDateTime { get; set; }
        public Station Station { get; set; }

    }
}
