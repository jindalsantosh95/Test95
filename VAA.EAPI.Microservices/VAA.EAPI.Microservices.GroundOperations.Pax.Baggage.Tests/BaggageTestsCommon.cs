using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.GroundOperations.Pax.Baggage.Tests
{
    public class BaggageTestsCommon
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"DeltaSsciEndpoint", "https://stg.virginatlantic.com/PCCOciWeb/apiv2"},
                {"DeltaAir4TokenEndpoint", "https://Test.stg.vs.air4.com/as/token.oauth2"},
                {"DeltaChannelValue", "TestChannelValue" },
                {"DeltaApiIdValue", "TestAppId"},
                {"DeltaClientIdValue", "Test_CC_Test"},
                {"DeltaClientSecret", "TestSecret"},
                {"BlobStorageUri", "https://test.blobstorage.net"},
                {"ContainerName", "TestContainerName"},
                {"AllAboutBagsBlob", "testblob.json"},
                {"SasKey", "TestKey"},
                {"BaggageUrl", "https://Test.virginatlantic.com/"},
                {"MicroServiceDBKey", "TestDbKey"},
                {"MicroServiceEventStoreTableName", "TestTableName"},
                {"EmailServiceBusConnectionString", "TestConnectionString"}
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value); ;
        }
    }
}
