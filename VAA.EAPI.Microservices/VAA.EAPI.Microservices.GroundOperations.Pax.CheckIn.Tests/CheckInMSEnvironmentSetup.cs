using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.CheckIn.Tests
{
    public class CheckInMSEnvironmentSetup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
