using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class Passenger
    {
        public Dictionary<string, object> Headers { get; set; }
        public PassengerFields Fields { get; set; }
        public PassengerSemantics Semantics { get; set; }
    }
}
