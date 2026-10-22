using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.FarePointCalc
{
    public class FareCalculationResponse
    {
        public int TotalFarePoints { get; set; } = 0;
        public IEnumerable<PassengerFare> PaxFares { get; set; }
    }
}
