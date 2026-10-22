using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class DeltaSSRResponse
    {
        public string Id { get; set; }
        public SpecialServiceRequests SpecialServiceRequests { get; set; }
        public IEnumerable<ErrorDetail> Errors { get; set; }
    }
}
