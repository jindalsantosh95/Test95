namespace VAA.EAPI.Microservices.Operations.Tests.AirportInfo
{
    public class OperationsTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"AirportInfo:Cache", "0.00:00:10"},
                {"AirportInfo:URL", "https://www.virginatlantic.com/util/airports/ALL/asc"},
                {"AirportInfo:Accept", "application/json"},
                {"AirportInfo:Agent", "Chrome/109.0.0.0"}
            };

            foreach (var item in settings)
            {
                Environment.SetEnvironmentVariable(item.Key, item.Value, EnvironmentVariableTarget.Process);
            }
        }
    }
}
