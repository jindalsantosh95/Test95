namespace VAA.EAPI.Automation.OpenAI.Tests.TestCases
{
    public class FlightSearchTestData
    {
        public static IEnumerable<object[]> TestCases()
        {
            yield return new object[]
            {
                   "Does Virgin Atlantic fly direct to Atlanta?",
                   "Virgin Atlantic does fly direct to Atlanta",
                   new[] { "Virgin Atlantic", "direct", "Atlanta" }
            };
            yield return new object[]
            {
                   "What cabin classes does Virgin Atlantic offer?",
                   "Virgin Atlantic offers the following cabin classes",
                   new[] { "Economy", "Premium", "Upper Class" }
            };
        }
    }
}
