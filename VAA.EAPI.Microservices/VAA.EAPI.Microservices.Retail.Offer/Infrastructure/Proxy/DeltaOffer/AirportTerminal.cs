using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy.DeltaOffer
{
    public class AirportTerminal
    {
        public IEnumerable<AirportGate> AirportGate { get; set; }
        public bool NoTerminalFound { get; set; }
        public string TerminalId { get; set; }
        public string TerminalName { get; set; }
    }
}
