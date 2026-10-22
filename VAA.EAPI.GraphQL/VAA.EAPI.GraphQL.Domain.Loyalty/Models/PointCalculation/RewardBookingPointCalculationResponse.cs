using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PointCalculation
{
    public class RewardBookingPointCalculationResponse
    {
        public int TotalFarePoints { get; set; } = 0;
        public IEnumerable<PassengerFare> PaxFares { get; set; }
    }
}
