using VAA.EAPI.GraphQL.Domain.PaxOps.Constants;

namespace VAA.EAPI.GraphQL.Domain.PaxOps.Infrastructure.Models.CheckInMS
{
    public class BoardingPassResponse
    {
        public IEnumerable<BoardingPass> BoardingPasses { get; set; }
        public PassengersCheckInStatus CheckInStatus { get; set; }
    }
}
