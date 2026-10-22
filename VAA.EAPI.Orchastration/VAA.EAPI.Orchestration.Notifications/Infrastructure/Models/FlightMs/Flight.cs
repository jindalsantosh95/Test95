using System.Collections.Generic;
namespace VAA.EAPI.Orchestration.Notifications.Infrastructure.Models.FlightMs
{
    public class Flight
    {
        public Aircraft Aircraft { get; set; }
        public string Carrier { get; set; }
        public string FlightNumber { get; set; }
        public IEnumerable<LegDetails> Legs { get; set; }
    }
}
