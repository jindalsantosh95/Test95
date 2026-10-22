using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class FlightRequestModel
    {
        public string FlightExternalId { get; set; }
        public IEnumerable<PassengerRequest> Passengers { get; set; }
    }
}
