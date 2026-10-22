using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Wallet.Domain.BoardingPass
{
    public class WalletPassRequest
    {
        public IEnumerable<AirportDetails> Airports { get; set; }
        public IEnumerable<Segment> Segments { get; set; }
        public IEnumerable<BoardingPass> BoardingPasses { get; set; }
        public IEnumerable<Passenger> Passengers { get; set; }
    }
}