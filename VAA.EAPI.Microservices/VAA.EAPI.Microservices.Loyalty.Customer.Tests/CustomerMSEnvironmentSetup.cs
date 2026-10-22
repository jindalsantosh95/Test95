using Microsoft.Azure.WebJobs.Description;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Tests
{
    public class CustomerMSEnvironmentSetup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"MicroServiceDBKey", "testvalue"},
                {"connectionstring","Testkey" },
                {"tabname","testvalue" } 
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}


