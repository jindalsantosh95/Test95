using System.Collections.Generic;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class UpdatePNRAirbook
    {
        public IEnumerable<HaltOnStatus> HaltOnStatus { get; set; }
        public OriginDestinationInformation OriginDestinationInformation { get; set; }
        public RedisplayReservation RedisplayReservation { get; set; }
    }
}
