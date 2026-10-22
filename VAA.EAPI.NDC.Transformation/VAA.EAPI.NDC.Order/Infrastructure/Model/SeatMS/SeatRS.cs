using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS
{
    public class SeatRS
    {
        public SeatAvailabilityResponse SeatAvailabilityResponse { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}
