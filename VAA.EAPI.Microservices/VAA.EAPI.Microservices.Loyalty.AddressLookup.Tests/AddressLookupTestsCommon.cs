using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Tests
{
    public class AddressLookupTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"EventTopicType", "AddressLookupCompletedEvent"},
                {"ServiceBusTopicName","enrolcustomer"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
            ConfigReader.Set("GbgFindEndpoint", "https://api.addressy.com/Capture/Interactive/Find/v1.20/json6.ws?Key={keyValue}&Text={textValue}");
            ConfigReader.Set("GbgRetrieveEndpoint", "https://api.addressy.com/Capture/Interactive/Retrieve/v1.30/json6.ws?Key={keyValue}&Id={IdValue}");
            ConfigReader.Set("GbgSearchRecordLimit", "20");
            ConfigReader.Set("GbgServiceKeyMap", "[{\"clientId\":\"some-id\",\"gbgKey\":\"id-specific-key\"},{\"clientId\":\"\",\"gbgKey\":\"DEFAULT_KEY\"}]");
        }
    }
}
