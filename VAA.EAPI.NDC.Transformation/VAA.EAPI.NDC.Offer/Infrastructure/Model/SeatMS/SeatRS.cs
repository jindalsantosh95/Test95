using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;

namespace VAA.EAPI.NDC.Offer.Infrastructure.Model.SeatMS
{
    public class SeatRS : SeatMSAvailabilityResponse
    {
        public SeatMSAvailabilityResponse SeatAvailabilityResponse { get; set; }
        public IEnumerable<ErrorDetails> Error { get; set; }
    }
}
