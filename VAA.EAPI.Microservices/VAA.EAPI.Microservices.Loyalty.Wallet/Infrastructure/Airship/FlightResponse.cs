using System;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Infrastructure.Airship
{
    public class FlightResponse
    {
        public int FlightId { get; set; }
        public int ProjectId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public FlightResponseFields Fields { get; set; }
        public string FlightExternalId { get; set; }
        public string ProjectExternalId { get; set; }
    }
}
