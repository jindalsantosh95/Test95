using System;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class Flight
    {
        public int FlightId { get; set; }
        public Dictionary<string, object> Fields { get; set; }
        public string FlightExternalId { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ProjectId { get; set; }
    }
}
