using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Trips.Helpers;

namespace VAA.EAPI.Automation.Trips.Tests.TripsTests.GraphQL
{
    public abstract class TripsGraphQlTestsBase
    {
        protected string env = Config.Environment;

        protected static void ValidatePnrDetails(Dictionary<string, string> pnrDetails)
            => TestDataPrerequisiteHelper.ValidatePnrDetailsOrSkip(pnrDetails);
    }
}
