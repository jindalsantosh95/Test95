using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class Payload
    {
        public IEnumerable<Flight> Flights { get; set; }
    }
}
