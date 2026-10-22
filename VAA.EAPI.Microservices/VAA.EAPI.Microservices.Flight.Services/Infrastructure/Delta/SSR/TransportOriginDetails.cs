using System;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class TransportOriginDetails
    {
        public DateTime ScheduledDepartureLocalDateTime { get; set; }
        public Station Station { get; set; }
    }
}
