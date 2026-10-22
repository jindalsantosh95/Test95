
namespace VAA.EAPI.GraphQL.Domain.Book.Models.InRuleEngine
{
    public class PointsEarnCalculatorRequest
    {
        public IEnumerable<EarnCalculatorFlightSegment> FlightSegments { get; set; }
        public IEnumerable<EarnCalculatorCustomerDetails> CustomerDetails { get; set; }
    }
}
