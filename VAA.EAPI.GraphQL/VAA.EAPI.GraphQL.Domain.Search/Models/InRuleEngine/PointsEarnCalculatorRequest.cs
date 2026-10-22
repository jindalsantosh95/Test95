namespace VAA.EAPI.GraphQL.Domain.Search.Models.InRuleEngine
{
    public class PointsEarnCalculatorRequest
    {
        public IEnumerable<EarnCalculatorFlightSegment>? FlightSegments { get; set; }
        public IEnumerable<EarnCalculatorCustomerDetails>? CustomerDetails { get; set; }
    }
}
