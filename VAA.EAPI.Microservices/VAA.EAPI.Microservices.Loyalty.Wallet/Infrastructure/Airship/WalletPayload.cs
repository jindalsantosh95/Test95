using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class WalletPayload
    {
        public IEnumerable<FlightRequestModel> Flights { get; set; }
    }
}
